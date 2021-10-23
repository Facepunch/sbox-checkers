using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
	partial class CheckersBoard : Entity
	{

		[Net]
		public Vector3 Mins { get; set; }
		[Net]
		public Vector3 Maxs { get; set; }

		public float CellSize => (Maxs.x - Mins.x) / 8;

		public CheckersBoard()
		{
			Transmit = TransmitType.Always;
		}

		public void SpawnCells()
		{
			if ( IsClient )
			{
				return;
			}

			foreach ( var child in Children )
			{
				child.Delete();
			}

			var pieceIdx = 0;

			for ( int y = 0; y < 8; y++ )
			{
				for ( int x = 0; x < 8; x++ )
				{
					var cell = Entity.Create<CheckersCell>();
					cell.SetParent( this );
					cell.Mins = new Vector3( CellSize * x, CellSize * y ) + Mins;
					cell.Maxs = (CellSize * Vector3.One).WithZ( 0 ) + cell.Mins;
					cell.BoardPosition = new Vector2( x, y );

					if((x + y) % 2 == 0)
					{
						if (pieceIdx >= 12 && pieceIdx < 20 )
						{
							pieceIdx++;
							continue;
						}
						pieceIdx++;
						var piece = Entity.Create<CheckersPiece>();
						piece.SetParent( this );
						piece.Position = cell.Center;
						piece.RenderColor = pieceIdx <= 12 ? Color.Black : Color.Red;
					}

				}
			}
		}

		public CheckersCell GetCellAt( Vector3 origin )
		{
			foreach ( var child in Children )
			{
				if ( child is not CheckersCell cell || !cell.Contains( origin ) )
				{
					continue;
				}
				return cell;
			}
			return null;
		}

	}
}
