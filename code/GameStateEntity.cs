using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
    partial class GameStateEntity : Entity
	{

		[Net]
		public GameState State { get; set; }

	}
}
