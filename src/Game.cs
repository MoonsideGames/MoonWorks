using SDL3;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MoonWorks.Input;
using System.Text;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MoonWorks.Storage;
using MoonWorks.Video;

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
		public AppInfo AppInfo { get; }

		public TimeSpan MAX_DELTA_TIME = TimeSpan.FromMilliseconds(100);
		public FramePacingSettings FramePacingSettings { get; private set; }

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

		public GraphicsDevice GraphicsDevice { get; }
		public AudioDevice AudioDevice { get; }
		public VideoDevice VideoDevice { get; }
		public Inputs Inputs { get; }

		/// <summary>
		/// Automatically opens on startup and automatically closes on shutdown.
		/// </summary>
		public TitleStorage RootTitleStorage { get; }

		public UserStorage UserStorage { get; }

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
			AppInfo appInfo,
			WindowCreateInfo windowCreateInfo,
			FramePacingSettings framePacingSettings,
			ShaderFormat availableShaderFormats,
			bool debugMode = false
		) {
			AppInfo = appInfo;

			Logger.LogInfo("Starting up MoonWorks...");
			Logger.LogInfo("Initializing frame limiter...");
			gameTimer = Stopwatch.StartNew();

			FramePacingSettings = framePacingSettings;

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

			Logger.InitSDLLogging();

			Logger.LogInfo("Initializing title storage...");
			RootTitleStorage = new TitleStorage(System.AppContext.BaseDirectory);

			Logger.LogInfo("Initializing user storage...");
			UserStorage = new UserStorage(AppInfo);

			Logger.LogInfo("Initializing input...");
			Inputs = new Inputs();

			Logger.LogInfo("Initializing graphics device...");
			GraphicsDevice = new GraphicsDevice(
				RootTitleStorage,
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

			Logger.LogInfo("Initializing video thread...");
			VideoDevice = new VideoDevice(GraphicsDevice);

			HandleSDLEvents(); // handle initial events so we can get initial controller settings, etc
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

			Logger.LogInfo($"Unclaiming and disposing windows...");
			foreach (var (id, window) in Window.IDToWindow)
			{
				GraphicsDevice.UnclaimWindow(window);
				window.Dispose();
			}

			Logger.LogInfo("Closing video thread...");
			VideoDevice.Dispose();

			Logger.LogInfo("Disposing graphics device...");
			GraphicsDevice.Dispose();

			Logger.LogInfo("Closing audio thread...");
			AudioDevice.Dispose();

			Logger.LogInfo("Disposing title storage...");
			RootTitleStorage.Dispose();

			Logger.LogInfo("Disposing user storage...");
			UserStorage.Dispose();

			Logger.LogInfo("Quitting SDL...");
			SDL.SDL_Quit();

			Logger.LogInfo("MoonWorks shutdown complete, see you next time!");
		}

		/// <summary>
		/// Updates the frame pacing settings.
		/// </summary>
		public void SetFramePacingSettings(FramePacingSettings settings)
		{
			FramePacingSettings = settings;
		}

		/// <summary>
		/// Starts the game shutdown process.
		/// </summary>
		public void Quit()
		{
			quit = true;
		}

		/// <summary>
        /// Executes once per tick, even when catching up.
        /// </summary>
		protected abstract void Step();

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

			if (FramePacingSettings.Mode != FramePacingMode.Uncapped)
			{
				// If we are in latency-optimized mode, we use the game timestep. Otherwise, we use the framerate cap.
				var capTimespan = FramePacingSettings.Mode == FramePacingMode.Capped ? FramePacingSettings.FramerateCapTimestep : FramePacingSettings.Timestep;

				/* We want to wait until the framerate cap,
				* but we don't want to oversleep. Requesting repeated 1ms sleeps and
				* seeing how long we actually slept for lets us estimate the worst case
				* sleep precision so we don't oversleep the next frame.
				*/
				while (accumulatedDrawTime + worstCaseSleepPrecision < capTimespan)
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
				while (accumulatedDrawTime < capTimespan)
				{
					System.Threading.Thread.SpinWait(1);
					AdvanceElapsedTime();
				}
			}

			if (FramePacingSettings.Mode == FramePacingMode.LatencyOptimized)
			{
				// Block on the swapchain before event processing for latency optimization.
				GraphicsDevice.WaitForSwapchain(MainWindow);
			}

			// Now that we are going to perform an update, let's handle SDL events.
			HandleSDLEvents();

			if (!quit)
			{
				Step();

				int updateCount = 0;
				while (accumulatedUpdateTime >= FramePacingSettings.Timestep && updateCount < FramePacingSettings.MaxUpdatesPerTick)
				{
					Inputs.Update();
					Update(FramePacingSettings.Timestep);

					accumulatedUpdateTime -= FramePacingSettings.Timestep;
					updateCount += 1;
				}

				if (updateCount > 1)
				{
					Logger.LogInfo($"Missed a frame, updated {updateCount} times, remaining accumulator time {accumulatedUpdateTime.TotalMilliseconds} ms");
				}

				AudioDevice.WakeThread();

				// Timestep alpha should be 0 if we are in latency-optimized mode.
				var alpha = FramePacingSettings.Mode == FramePacingMode.LatencyOptimized ?
					0 :
					(accumulatedUpdateTime / FramePacingSettings.Timestep);


				Draw(alpha);
				accumulatedDrawTime -= FramePacingSettings.FramerateCapTimestep;
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

					case (uint) SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
					case (uint) SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
						HandleMouseButton(_event.button);
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
						HandleGamepadAdded(_event.gdevice);
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED:
						HandleGamepadRemoved(_event.gdevice);
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN:
					case (uint) SDL.SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP:
						HandleGamepadButton(_event.gbutton);
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION:
						HandleGamepadAxis(_event.gaxis);
						break;

					case (uint) SDL.SDL_EventType.SDL_EVENT_KEY_DOWN:
					case (uint) SDL.SDL_EventType.SDL_EVENT_KEY_UP:
						HandleKeyboardButton(_event.key);
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

			if (window.Handle == MainWindow.Handle)
			{
				// If the main window is closing, close the app.
				Quit();
			}
			else
			{
				// Otherwise, unclaim and dispose.
				GraphicsDevice.UnclaimWindow(window);
				window.Dispose();
			}
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

		private void HandleGamepadAdded(SDL.SDL_GamepadDeviceEvent evt)
		{
			var index = evt.which;
			if (SDL.SDL_IsGamepad(index))
			{
				Logger.LogInfo("New controller detected...");
				Inputs.AddGamepad(index);
			}
		}

		private void HandleGamepadRemoved(SDL.SDL_GamepadDeviceEvent evt)
		{
			Logger.LogInfo("Controller removal detected!");
			Inputs.RemoveGamepad(evt.which);
		}

		private void HandleGamepadButton(SDL.SDL_GamepadButtonEvent evt)
        {
			var index = evt.which;
			var gamepad = Inputs.GetGamepadFromJoystickID(index);
			gamepad?.AddButtonEvent(evt);
        }

		private void HandleGamepadAxis(SDL.SDL_GamepadAxisEvent evt)
        {
			var index = evt.which;
			var gamepad = Inputs.GetGamepadFromJoystickID(index);
			gamepad?.AddAxisEvent(evt);
        }

		private void HandleKeyboardButton(SDL.SDL_KeyboardEvent evt)
        {
            Inputs.Keyboard.AddButtonEvent(evt);
        }

		private void HandleMouseButton(SDL.SDL_MouseButtonEvent evt)
        {
            Inputs.Mouse.AddButtonEvent(evt);
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
