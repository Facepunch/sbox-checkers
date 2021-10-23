using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
    partial class CheckersPiece : ModelEntity
	{

		public override void Spawn()
		{
			base.Spawn();

			//Transmit = TransmitType.Always;

			SetModel( "models/checkers_piece.vmdl" );
		}

	}
}
