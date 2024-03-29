﻿
namespace Facepunch.Checkers;

public class CheckersBaseNameTag : Panel
{
	public Label NameLabel;
	public Image Avatar;

	public CheckersBaseNameTag( CheckersPlayer player )
	{
		var client = player.Client;

		NameLabel = Add.Label( $"{client.Name}" );
		Avatar = Add.Image( $"avatar:{client.SteamId}" );
	}

	public virtual void UpdateFromPlayer( CheckersPlayer player )
	{
		// Nothing to do unless we're showing health and shit
	}
}

[StyleSheet]
class CheckersNametags : Panel
{

	Dictionary<CheckersPlayer, CheckersBaseNameTag> ActiveTags = new Dictionary<CheckersPlayer, CheckersBaseNameTag>();

	public const float MaxDrawDistance = 300;

	public CheckersNametags()
	{
		StyleSheet.Load( "/ui/CheckersNameTags.scss" );
	}

	public override void Tick()
	{
		base.Tick();

		var deleteList = new List<CheckersPlayer>();
		deleteList.AddRange( ActiveTags.Keys );

		int count = 0;
		foreach ( var player in Entity.All.OfType<CheckersPlayer>().OrderBy( x => Vector3.DistanceBetween( x.Position, Camera.Position ) ) )
		{
			if ( UpdateNameTag( player ) )
			{
				deleteList.Remove( player );
				count++;
			}
		}

		foreach ( var player in deleteList )
		{
			ActiveTags[player].Delete();
			ActiveTags.Remove( player );
		}
	}

	public virtual CheckersBaseNameTag CreateNameTag( CheckersPlayer player )
	{
		if ( player.Client == null )
			return null;

		var tag = new CheckersBaseNameTag( player );
		tag.Parent = this;
		return tag;
	}

	public bool UpdateNameTag( CheckersPlayer player )
	{
		var labelPos = player.GetAttachment( "hat" ).Value.Position + Vector3.Up * 50;

		if ( Game.LocalPawn is not CheckersPlayer p ) return false;

		var screeenRay = new Ray( Camera.Position, p.CursorDirection );

		var tr = Trace.Ray( screeenRay, 5000 )
		.StaticOnly()
		.Run();

		if ( !tr.Hit )
		{
			return false;
		}

		float dist = labelPos.Distance( tr.EndPosition );

		if ( Game.LocalPawn == player )
		{
			dist = 0;
		}

		if ( dist > MaxDrawDistance )
			return false;

		var alpha = dist.LerpInverse( MaxDrawDistance, MaxDrawDistance * 0.1f, true );
		var objectSize = .05f / dist / (2.0f * MathF.Tan( (Camera.FieldOfView / 2.0f).DegreeToRadian() )) * 1500.0f;

		objectSize = objectSize.Clamp( 0.05f, .3f );

		if ( !ActiveTags.TryGetValue( player, out var tag ) )
		{
			tag = CreateNameTag( player );
			if ( tag != null )
			{
				ActiveTags[player] = tag;
			}
		}

		if ( tag == null )
			return false;

		tag.UpdateFromPlayer( player );

		var screenPos = labelPos.ToScreen();

		tag.Style.Left = Length.Fraction( screenPos.x );
		tag.Style.Top = Length.Fraction( screenPos.y );
		tag.Style.Opacity = alpha;

		var transform = new PanelTransform();
		transform.AddTranslateY( Length.Fraction( -1.0f ) );
		transform.AddScale( objectSize );
		transform.AddTranslateX( Length.Fraction( -0.5f ) );

		tag.Style.Transform = transform;
		tag.Style.Dirty();

		return true;
	}

}
