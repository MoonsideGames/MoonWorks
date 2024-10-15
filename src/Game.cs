using SDL3;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MoonWorks.Input;
using System.Text;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MoonWorks
{
	/// <summary>
	/// This class is your entry point into controlling your game. <br/>
	/// It manages the main game loop and subsystems. <br/>
	/// You should inherit this class and implement Update and Draw methods. <br/>
	/// Then instantiate your Game subclass from your Program.Main method and call the Run method.
	/// </summary>
	public abstract class Game
	{
		public TimeSpan MAX_DELTA_TIME = TimeSpan.FromMilliseconds(100);
		public TimeSpan Timestep { get; private set; }

		private bool quit = false;
		private Stopwatch gameTimer;
		private long previousTicks = 0;
		TimeSpan accumulatedUpdateTime = TimeSpan.Zero;
		TimeSpan accumulatedDrawTime = TimeSpan.Zero;
		// must be a power of 2 so we can do a bitmask optimization when checking worst case
		private const int PREVIOUS_SLEEP_TIME_COUNT = 128;
		private const int SLEEP_TIME_MASK = PREVIOUS_SLEEP_TIME_COUNT - 1;
		private TimeSpan[] previousSleepTimes = new TimeSpan[PREVIOUS_SLEEP_TIME_COUNT];
		private int sleepTimeIndex = 0;
		private TimeSpan worstCaseSleepPrecision = TimeSpan.FromMilliseconds(1);

		private bool FramerateCapped = false;
		private TimeSpan FramerateCapTimeSpan = TimeSpan.Zero;

		public GraphicsDevice GraphicsDevice { get; }
		public AudioDevice AudioDevice { get; }
		public Inputs Inputs { get; }

		/// <summary>
		/// This Window is automatically created when your Game is instantiated.
		/// </summary>
		public Window MainWindow { get; }

		/// <summary>
		/// Instantiates your Game.
		/// </summary>
		/// <param name="windowCreateInfo">The parameters that will be used to create the MainWindow.</param>
		/// <param name="frameLimiterSettings">The frame limiter settings.</param>
		/// <param name="availableShaderFormats">Bitflags of which GPU backends to attempt to initialize.</param>
		/// <param name="targetTimestep">How often Game.Update will run in terms of ticks per second.</param>
		/// <param name="debugMode">If true, enables extra debug checks. Should be turned off for release builds.</param>
		public Game(
			WindowCreateInfo windowCreateInfo,
			FrameLimiterSettings frameLimiterSettings,
			ShaderFormat availableShaderFormats,
			int targetTimestep = 60,
			bool debugMode = false
		) {
			Logger.LogInfo("Initializing frame limiter...");
			Timestep = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / targetTimestep);
			gameTimer = Stopwatch.StartNew();

			SetFrameLimiter(frameLimiterSettings);

			for (int i = 0; i < previousSleepTimes.Length; i += 1)
			{
				previousSleepTimes[i] = TimeSpan.FromMilliseconds(1);
			}

			Logger.LogInfo("Initializing SDL...");
			if (!SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO | SDL.SDL_InitFlags.SDL_INIT_TIMER | SDL.SDL_InitFlags.SDL_INIT_GAMEPAD))
			{
				Logger.LogError("Failed to initialize SDL!");
				return;
			}

			Logger.Init();

			Logger.LogInfo("Initializing input...");
			Inputs = new Inputs();

			Logger.LogInfo("Initializing graphics device...");
			GraphicsDevice = new GraphicsDevice(
				availableShaderFormats,
				debugMode
			);

			Logger.LogInfo("Initializing main window...");
			MainWindow = new Window(windowCreateInfo, SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN);

			if (!GraphicsDevice.ClaimWindow(MainWindow))
			{
				throw new System.SystemException("Could not claim window!");
			}

			Logger.LogInfo("Initializing audio thread...");
			AudioDevice = new AudioDevice();
		}

		/// <summary>
		/// Initiates the main game loop. Call this once from your Program.Main method.
		/// </summary>
		public void Run()
		{
			MainWindow.Show();

			while (!quit)
			{
				Tick();
			}

			Logger.LogInfo("Starting shutdown sequence...");

			Logger.LogInfo("Cleaning up game...");
			Destroy();

			Logger.LogInfo("Unclaiming window...");
			GraphicsDevice.UnclaimWindow(MainWindow);

			Logger.LogInfo("Disposing window...");
			MainWindow.Dispose();

			Logger.LogInfo("Disposing graphics device...");
			GraphicsDevice.Dispose();

			Logger.LogInfo("Closing audio thread...");
			AudioDevice.Dispose();

			Logger.LogInfo("Quitting SDL...");
			SDL.SDL_Quit();

			Logger.LogInfo("MoonWorks shutdown complete, see you next time!");
		}

		/// <summary>
		/// Updates the frame limiter settings.
		/// </summary>
		public void SetFrameLimiter(FrameLimiterSettings settings)
		{
			FramerateCapped = settings.Mode == FrameLimiterMode.Capped;

			if (FramerateCapped)
			{
				FramerateCapTimeSpan = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / settings.Cap);
			}
			else
			{
				FramerateCapTimeSpan = TimeSpan.Zero;
			}
		}

		/// <summary>
		/// Starts the game shutdown process.
		/// </summary>
		public void Quit()
		{
			quit = true;
		}

		/// <summary>
		/// Will execute at the specified targetTimestep you provided when instantiating your Game class.
		/// </summary>
		/// <param name="delta"></param>
		protected abstract void Update(TimeSpan delta);

		/// <summary>
		/// If the frame limiter mode is Capped, this will run at most Cap times per second. <br />
		/// Otherwise it will run as many times as possible.
		/// </summary>
		/// <param name="alpha">A value from 0-1 describing how "in-between" update ticks it is called. Useful for interpolation.</param>
		protected abstract void Draw(double alpha);

		/// <summary>
		/// You can optionally override this to perform cleanup tasks before the game quits.
		/// </summary>
		protected virtual void Destroy() {}

		/// <summary>
		/// Called when a file is dropped on the game window.
		/// </summary>
		protected virtual void DropFile(string filePath) {}

		/// <summary>
		/// Required to distinguish between multiple files dropped at once
		/// vs multiple files dropped one at a time.
		/// Called once for every multi-file drop.
		/// </summary>
		protected virtual void DropBegin() {}
		protected virtual void DropComplete() {}

		private void Tick()
		{
			AdvanceElapsedTime();

			if (FramerateCapped)
			{
				/* We want to wait until the framerate cap,
				* but we don't want to oversleep. Requesting repeated 1ms sleeps and
				* seeing how long we actually slept for lets us estimate the worst case
				* sleep precision so we don't oversleep the next frame.
				*/
				while (accumulatedDrawTime + worstCaseSleepPrecision < FramerateCapTimeSpan)
				{
					System.Threading.Thread.Sleep(1);
					TimeSpan timeAdvancedSinceSleeping = AdvanceElapsedTime();
					UpdateEstimatedSleepPrecision(timeAdvancedSinceSleeping);
				}

				/* Now that we have slept into the sleep precision threshold, we need to wait
				* for just a little bit longer until the target elapsed time has been reached.
				* SpinWait(1) works by pausing the thread for very short intervals, so it is
				* an efficient and time-accurate way to wait out the rest of the time.
				*/
				while (accumulatedDrawTime < FramerateCapTimeSpan)
				{
					System.Threading.Thread.SpinWait(1);
					AdvanceElapsedTime();
				}
			}

			// Now that we are going to perform an update, let's handle SDL events.
			HandleSDLEvents();

			// Do not let any step take longer than our maximum.
			if (accumulatedUpdateTime > MAX_DELTA_TIME)
			{
				accumulatedUpdateTime = MAX_DELTA_TIME;
			}

			if (!quit)
			{
				while (accumulatedUpdateTime >= Timestep)
				{
					Inputs.Update();
					Update(Timestep);
					AudioDevice.WakeThread();

					accumulatedUpdateTime -= Timestep;
				}

				var alpha = accumulatedUpdateTime / Timestep;

				Draw(alpha);
				accumulatedDrawTime -= FramerateCapTimeSpan;
			}
		}

		private void HandleSDLEvents()
		{
			while (SDL.SDL_PollEvent(out var _event))
			{
				switch (_event.type)
				{
					case (uint) SDL.SDL_EventType.SDL_EVENT_QUIT:
						quit = true;
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_TEXT_INPUT:
						HandleTextInput(_event.text);
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
						Inputs.Mouse.WheelRaw += (int) _event.wheel.y;
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_DROP_BEGIN:
						DropBegin();
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_DROP_COMPLETE:
						DropComplete();
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_DROP_FILE:
						HandleFileDrop(_event.drop);
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_GAMEPAD_ADDED:
						HandleControllerAdded(_event.gdevice);
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED:
						HandleControllerRemoved(_event.gdevice);
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED:
						HandleWindowPixelSizeChangeEvent(_event.window);
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
						HandleWindowCloseRequestedEvent(_event.window);
						break;
				}
			}
		}

		private void HandleWindowPixelSizeChangeEvent(SDL.SDL_WindowEvent evt)
		{
			var window = Window.Lookup(evt.windowID);
			window.HandleSizeChange((uint) evt.data1, (uint) evt.data2);
		}

		private void HandleWindowCloseRequestedEvent(SDL.SDL_WindowEvent evt)
		{
			var window = Window.Lookup(evt.windowID);
			GraphicsDevice.UnclaimWindow(window);
			window.Dispose();
		}

		private void HandleTextInput(SDL.SDL_TextInputEvent evt)
		{
			// Based on the SDL2# LPUtf8StrMarshaler
			unsafe
			{
				int bytes = MeasureStringLength(evt.text);
				if (bytes > 0)
				{
					/* UTF8 will never encode more characters
                        * than bytes in a string, so bytes is a
                        * suitable upper estimate of size needed
                        */
					char* charsBuffer = stackalloc char[bytes];
					int chars = Encoding.UTF8.GetChars(
						evt.text,
						bytes,
						charsBuffer,
						bytes
					);

					for (int i = 0; i < chars; i += 1)
					{
						Inputs.OnTextInput(charsBuffer[i]);
					}
				}
			}
		}

		private unsafe void HandleFileDrop(SDL.SDL_DropEvent evt)
		{
			var filePath = Marshal.PtrToStringUTF8((nint) evt.data);
			DropFile(filePath);
		}

		private void HandleControllerAdded(SDL.SDL_GamepadDeviceEvent evt)
		{
			var index = evt.which;
			if (SDL.SDL_IsGamepad(index))
			{
				Logger.LogInfo("New controller detected...");
				Inputs.AddGamepad(index);
			}
		}

		private void HandleControllerRemoved(SDL.SDL_GamepadDeviceEvent evt)
		{
			Logger.LogInfo("Controller removal detected!");
			Inputs.RemoveGamepad(evt.which);
		}

		public static void ShowRuntimeError(string title, string message)
		{
			SDL.SDL_ShowSimpleMessageBox(
				SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
				title ?? "",
				message ?? "",
				IntPtr.Zero
			);
		}

		private TimeSpan AdvanceElapsedTime()
		{
			long currentTicks = gameTimer.Elapsed.Ticks;
			TimeSpan timeAdvanced = TimeSpan.FromTicks(currentTicks - previousTicks);
			accumulatedUpdateTime += timeAdvanced;
			accumulatedDrawTime += timeAdvanced;
			previousTicks = currentTicks;
			return timeAdvanced;
		}

		/* To calculate the sleep precision of the OS, we take the worst case
		 * time spent sleeping over the results of previous requests to sleep 1ms.
		 */
		private void UpdateEstimatedSleepPrecision(TimeSpan timeSpentSleeping)
		{
			/* It is unlikely that the scheduler will actually be more imprecise than
			 * 4ms and we don't want to get wrecked by a single long sleep so we cap this
			 * value at 4ms for sanity.
			 */
			var upperTimeBound = TimeSpan.FromMilliseconds(4);

			if (timeSpentSleeping > upperTimeBound)
			{
				timeSpentSleeping = upperTimeBound;
			}

			/* We know the previous worst case - it's saved in worstCaseSleepPrecision.
			 * We also know the current index. So the only way the worst case changes
			 * is if we either 1) just got a new worst case, or 2) the worst case was
			 * the oldest entry on the list.
			 */
			if (timeSpentSleeping >= worstCaseSleepPrecision)
			{
				worstCaseSleepPrecision = timeSpentSleeping;
			}
			else if (previousSleepTimes[sleepTimeIndex] == worstCaseSleepPrecision)
			{
				var maxSleepTime = TimeSpan.MinValue;
				for (int i = 0; i < previousSleepTimes.Length; i++)
				{
					if (previousSleepTimes[i] > maxSleepTime)
					{
						maxSleepTime = previousSleepTimes[i];
					}
				}
				worstCaseSleepPrecision = maxSleepTime;
			}

			previousSleepTimes[sleepTimeIndex] = timeSpentSleeping;
			sleepTimeIndex = (sleepTimeIndex + 1) & SLEEP_TIME_MASK;
		}

		private unsafe static int MeasureStringLength(byte* ptr)
		{
			int bytes;
			for (bytes = 0; *ptr != 0; ptr += 1, bytes += 1) ;
			return bytes;
		}
	}
}
