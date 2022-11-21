using Assets.Crimson.Core.Common;
using Assets.Crimson.Core.Components;
using Assets.Crimson.Core.Components.UI;
using Crimson.Core.Components;
using Unity.Entities;
using UnityEngine;

namespace Assets.Crimson.Core.Systems
{
	public class UIFollowSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.WithAll<AbilityUIFollowAim>().ForEach(
				(AbilityUIFollowAim ability) =>
				{
					var aimData = EntityManager.GetComponentData<AimData>(ability.Owner.ActorEntity);
					var ownerPosition = Camera.main.WorldToScreenPoint(ability.Owner.transform.position);
					var position = Camera.main.WorldToScreenPoint(aimData.RealPosition);
					var hasPostion = (ownerPosition - position).magnitude > ability.HideRadius;
					ability.CanvasGroup.alpha = hasPostion ? 1 : 0;
					if (hasPostion)
					{
						ability.Mark.anchoredPosition = position;
					}
				});

			Entities.WithAll<AbilityUIFollowMouse>().ForEach(
			(AbilityUIFollowMouse ability) =>
			{
				var data = EntityManager.GetComponentData<PlayerInputData>(ability.Owner.ActorEntity);
				var ownerPosition = (Vector2)Camera.main.WorldToScreenPoint(ability.Owner.transform.position);
				var position = (Vector2)data.Mouse;
				var hasPostion = (ownerPosition - position).magnitude > ability.HideRadius;
				ability.CanvasGroup.alpha = hasPostion ? 1 : 0;
				if (hasPostion)
				{
					ability.TargetRect.anchoredPosition = position;
				}
			});
		}
	}
}