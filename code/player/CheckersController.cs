using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Checkers
{
    class CheckersController : WalkController
	{

		public override void FrameSimulate()
		{
			base.FrameSimulate();

			if (Pawn is not CheckersPlayer p) return;

            var screeenRay = new Ray(p.CursorPosition, p.CursorDirection);

            var tr = Trace.Ray(screeenRay, 3000 )
				.WorldOnly()
				.Run();

			if ( tr.Hit )
			{
				var lookDir = (tr.EndPosition - Pawn.Position).WithZ( 0 ).Normal;
				Rotation = Rotation.LookAt( lookDir, Vector3.Up );
			}
		}

		public override void Simulate()
		{
			var player = Pawn as CheckersPlayer;
			if (!player.IsValid()) return;

			EyeLocalPosition = Vector3.Up * (EyeHeight * Pawn.Scale);
			UpdateBBox();

			EyeLocalPosition += TraceOffset;
			EyeRotation = player.ViewAngles.ToRotation();

			if ( Unstuck.TestAndFix() )
				return;

			CheckLadder();

			if ( !Swimming )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
				Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

				BaseVelocity = BaseVelocity.WithZ( 0 );
			}

			if ( AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
			{
				CheckJumpButton();
			}

			bool bStartOnGround = GroundEntity != null;
			if ( bStartOnGround )
			{
				Velocity = Velocity.WithZ( 0 );

				if ( GroundEntity != null )
				{
					ApplyFriction( GroundFriction * SurfaceFriction );
				}
			}

			WishVelocity = new Vector3( player.InputDirection.x, player.InputDirection.y, 0 );
			var inSpeed = WishVelocity.Length.Clamp( 0, 1 );

			// todo: this kinda sucks
			var pl = Pawn as CheckersPlayer;
			var yaw = pl.Team == CheckersTeam.Black || pl.Team == CheckersTeam.Spectator ? 90 : -90;
			WishVelocity *= Rotation.FromYaw( yaw );

			if ( !Swimming )
			{
				WishVelocity = WishVelocity.WithZ( 0 );
			}

			WishVelocity = WishVelocity.Normal * inSpeed;
			WishVelocity *= GetWishSpeed();

			Duck.PreTick();

			bool bStayOnGround = false;
			if ( GroundEntity != null )
			{
				bStayOnGround = true;
				WalkMove();
			}
			else
			{
				AirMove();
			}

			CategorizePosition( bStayOnGround );

			// FinishGravity
			if ( !Swimming )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			}

			if ( GroundEntity != null )
			{
				Velocity = Velocity.WithZ( 0 );
			}
		}

	}
}
