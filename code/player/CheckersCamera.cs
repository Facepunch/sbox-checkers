using Sandbox;

namespace Facepunch.Checkers
{
	class CheckersCamera : CameraMode
	{

		public override void Update()
		{
			if ( Local.Pawn is not CheckersPlayer player )
			{
				return;
			}

			var targetCam = player.Team != CheckersTeam.Red 
				? "camera_black"
				: "camera_red";

			// todo: maybe calculate the ideal camera position using the board's bounds
			var cam = Entity.FindByName( targetCam );
			Position = cam.Position;
			Rotation = cam.Rotation;
			FieldOfView = 70;
		}

	}
}
