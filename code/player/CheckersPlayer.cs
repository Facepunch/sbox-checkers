
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Checkers
{
	public partial class CheckersPlayer : AnimatedEntity
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

        public override void Spawn()
        {
            base.Spawn();

            SetModel("models/citizen/citizen.vmdl");
            EnableAllCollisions = true;
            EnableDrawing = true;
            EnableHideInFirstPerson = true;
            EnableShadowInFirstPerson = true;

			MoveToSpawnPoint();
        }

		public void Dress(IClient client)
		{
			_clothing ??= new();
			_clothing.LoadFromClient(client);
			_clothing.DressEntity(this);
		}

		void MoveToSpawnPoint()
		{
            var spawnpoint = All
				.OfType<SpawnPoint>()               // get all SpawnPoint entities
				.OrderBy(x => Game.Random.Int(9999))     // order them by random
				.FirstOrDefault();                  // take the first one

            if (spawnpoint == null)
            {
                return;
            }

            Transform = spawnpoint.Transform;
        }

		public override void Simulate(IClient cl )
		{
			base.Simulate( cl );

            if ( !CheckersBoard.Current.IsValid() )
				return;

			var screeenRay = new Ray(CursorPosition, CursorDirection);

            var tr = Trace.Ray(screeenRay, 3000 )
				.StaticOnly()
				.Run();

			//DebugOverlay.Sphere(tr.EndPosition, 2.0f, Host.Color, 2.0f);

			if ( tr.Hit )
			{
				SetHoveredCell( CheckersBoard.Current.GetCellAt( tr.EndPosition ) );
			}

			if ( !HoveredCell.IsValid() )
			{
				return;
			}

			if ( Input.Pressed("click") )
			{
				var piece = CheckersBoard.Current.GetPieceAt( HoveredCell.BoardPosition );
				SetSelectedPiece( piece );
			}

			if ( !SelectedPiece.IsValid() )
			{
				return;
			}

			SelectedPiece.DragPosition = tr.Hit ? tr.EndPosition : Vector3.Zero;

			if ( Input.Released("click") )
			{
				if ( !Game.IsClient )
				{
					CheckersGame.Instance.AttemptMove( this, SelectedPiece, HoveredCell.BoardPosition );
				}

				SetSelectedPiece( null );
			}
		}

        
        public override void BuildInput()
		{
			CursorPosition = Camera.Position;
            CursorDirection = Mouse.Visible ? Screen.GetDirection( Mouse.Position ) : Camera.Rotation.Forward;

            worldInput.Ray = new Ray(Camera.Position, CursorDirection );
			worldInput.MouseLeftPressed = Input.Down("click");
			worldInput.MouseRightPressed = Input.Down("rightclick");
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
