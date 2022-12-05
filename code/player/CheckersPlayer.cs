using Sandbox;
using Sandbox.UI;
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
		[Net]
		public bool ReadyToStart { get; set; }
		[Net, Predicted]
		public CheckersPiece SelectedPiece { get; set; }
		[Net, Predicted]
		public CheckersCell HoveredCell { get; set; }

		[ClientInput]
		public Vector3 CursorDirection { get; set; }
        [ClientInput]
        public Vector3 CursorPosition { get; set; }

        private ClothingContainer _clothing = new();
		private WorldInput worldInput = new();

		public List<CheckersMove> LegalMoveCache = new List<CheckersMove>();

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			CameraMode = new CheckersCamera();
			Animator = new StandardPlayerAnimator();
			Controller = new CheckersController();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			if ( Client != null )
			{
				_clothing.LoadFromClient( Client );
				_clothing.DressEntity( this );
			}

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			var screeenRay = new Ray(CursorPosition, CursorDirection);

            var tr = Trace.Ray(screeenRay, 3000 )
				.WorldOnly()
				.Run();

			DebugOverlay.TraceResult(tr);
			DebugOverlay.Text(IsClient.ToString(), tr.EndPosition, 0, 5000);

			if ( tr.Hit )
			{
				SetHoveredCell( CheckersBoard.Current.GetCellAt( tr.EndPosition ) );
			}

			if ( !HoveredCell.IsValid() )
			{
				return;
			}

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				var piece = CheckersBoard.Current.GetPieceAt( HoveredCell.BoardPosition );
				SetSelectedPiece( piece );
			}

			if ( !SelectedPiece.IsValid() )
			{
				return;
			}

			SelectedPiece.DragPosition = tr.Hit ? tr.EndPosition : Vector3.Zero;

			if ( Input.Released( InputButton.PrimaryAttack ) )
			{
				if ( !IsClient )
				{
					CheckersGame.Instance.AttemptMove( this, SelectedPiece, HoveredCell.BoardPosition );
				}

				SetSelectedPiece( null );
			}
		}

		public override void BuildInput()
		{
			CursorPosition = CurrentView.Position;
            CursorDirection = Mouse.Visible? Screen.GetDirection(Mouse.Position) : CurrentView.Rotation.Forward;

            worldInput.Ray = new Ray( CurrentView.Position, CursorDirection );
			worldInput.MouseLeftPressed = Input.Down( InputButton.PrimaryAttack );
			worldInput.MouseRightPressed = Input.Down( InputButton.SecondaryAttack );
			worldInput.MouseScroll = Input.MouseWheel;
		}

		private void SetSelectedPiece( CheckersPiece piece )
		{
			if ( SelectedPiece.IsValid() )
			{
				SelectedPiece.Floating = false;
				SelectedPiece.DragPosition = Vector3.Zero;
			}

			if( !piece.IsValid() || piece.Team != Team )
			{
				SelectedPiece = null;
				return;
			}

			SelectedPiece = piece;
			SelectedPiece.Floating = true;
		}

		private void SetHoveredCell( CheckersCell cell )
		{
			if( !SelectedPiece.IsValid() )
			{
				LegalMoveCache.Clear();
			}

			if ( HoveredCell != null )
			{
				HoveredCell.Hovered = false;
			}

			HoveredCell = cell;

			if ( cell != null )
			{
				cell.Hovered = true;

				var piece = (cell.Parent as CheckersBoard).GetPieceAt( cell.BoardPosition );
				if ( piece.IsValid() && piece.Team == Team )
				{
					LegalMoveCache.AddRange( piece.GetLegalMoves() );
				}
			}
		}

	}
}
