using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class CheckersCell : Entity
	{

		[Net]
		public Vector2 BoardPosition { get; set; }
		[Net]
		public Vector3 Mins { get; set; }
		[Net]
		public Vector3 Maxs { get; set; }

		public Vector3 Center => (Maxs + Mins) / 2;
		public Color Color => (BoardPosition.x + BoardPosition.y) % 2 == 0 ? Color.Black : Color.Red;

		public CheckersCell()
		{
			Transmit = TransmitType.Always;
		}

		[Event.Frame]
		public void OnFrame()
		{
			//DebugOverlay.Box( Mins, Maxs.WithZ( Mins.z + 2 ), Color );
			//DebugOverlay.Text( Center, BoardPosition.ToString(), Color.White, 0, 2000 );
		}

	}
}
