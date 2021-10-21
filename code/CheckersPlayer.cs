using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class CheckersPlayer : Sandbox.Player
	{

		[Net]
		public CheckersTeam Team { get; set; } = CheckersTeam.Spectator;

		public CheckersPlayer()
		{
			Camera = new CheckersCamera();
		}

	}
}
