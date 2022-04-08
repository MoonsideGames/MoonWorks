using System;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MoonWorks.Input;

namespace MoonWorks
{
    public abstract class GameState
    {
		protected readonly Game Game;

		public Window Window => Game.Window;
		public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
		public AudioDevice AudioDevice => Game.AudioDevice;
		public Inputs Inputs => Game.Inputs;

		public GameState(Game game)
        {
			Game = game;
		}

		public abstract void Start();
		public abstract void Update(TimeSpan delta);
		public abstract void Draw(TimeSpan delta, double alpha);
	}
}
