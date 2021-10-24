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

		public bool Hovered { get; set; }

		public Vector3 Center => (Maxs + Mins) / 2;
		public Color Color => (BoardPosition.x + BoardPosition.y) % 2 == 0 ? Color.Black : Color.Red;

		public CheckersCell()
		{
			Transmit = TransmitType.Always;
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			new CellWorldPanel( this );
		}

		public bool Contains( Vector3 p )
		{
			if ( Mins.x <= p.x && p.x <= Maxs.x && Mins.y <= p.y && p.y <= Maxs.y )
			{
				return true;
			}
			return false;
		}

		[Event.Frame]
		public void OnFrame()
		{
			//DebugOverlay.Text( Center, BoardPosition.ToString(), Color.White, 0, 2000 );

			if ( Hovered )
			{
				//DebugOverlay.Sphere( Center, 50, Color.White );
			}
		}

	}
}
