using Actions.Components;
using Data;
using Data.Parameters;
using Data.Parameters.PlayerBullet;
using Esc.Game.Components.Tags;
using Game.Components;
using Game.Components.SpawnPoints;
using Game.Components.Tags;
using Game.Extensions;
using Infrastructure;
using Leopotam.Ecs;
using UnityEngine;

namespace Actions.Systems
{
    public class StartPlayerLaserAttackSystem : IEcsRunSystem
    {
        private readonly IPlayerBulletParameters _playerBulletParameters = null;
        
        private readonly CustomEcsWorld _world = null;

        private readonly EcsFilter<StartPlayerLaserAttackComponent> _actionGroup = null;
        private readonly EcsFilter<PlayerTagComponent, SpawnPointsWithBoolComponent, AdditionalWeaponComponent> _weaponsGroup = null;

        public void Run()
        {
            foreach (var index in _actionGroup)
            {
                foreach (var weaponIndex in _weaponsGroup)
                {
                    var weaponEntity = _weaponsGroup.GetEntity(weaponIndex);
                    
                    var chargesNumber = weaponEntity.Get<ChargesComponent>().Value;
                    
                    if(chargesNumber <= 0)
                        continue;
                    
                    var spawnPointsComponent = weaponEntity.Get<SpawnPointsWithBoolComponent>();
                    var spawnPoints = spawnPointsComponent.Value;

                    foreach (var point in spawnPoints)
                    {
                        var bullet = _world.CreateLaserBullet(point.Point.position,
                            point.Point.transform.TransformPoint(Vector3.up * 20), _playerBulletParameters.DamageLayerMask);
                        bullet.Replace(new DestroyDelayComponent() { Value = 0.1f });
                        
                        var damageLayer = bullet.Get<DamageLayerComponent>().Value;

                        var hits = Physics2D.RaycastAll(point.Point.position, point.Point.transform.TransformVector(Vector3.up), 20, damageLayer);
                        foreach (var hit in hits)
                        {
                            var uid = hit.collider.gameObject.GetInstanceID();
                            var hitEntity = _world.GetEntityWithUid(uid);
                            hitEntity.Get<DestroyComponent>();
                            hitEntity.Get<KilledTagComponent>();
                        }
                    }

                    var newChargesNumber = chargesNumber - 1;
                    _world.LaserChargeChange(newChargesNumber);
                    var chargesComponent = new ChargesComponent() { Value = newChargesNumber };
                    weaponEntity.Replace(chargesComponent);
                }
            }
        }
    }
}