//##################################################################################################
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//##################################################################################################

using UnityEngine;

//##################################################################################################
// Gun Component
// This class is responsible for using gun data to spawn bullet prefabs when 'shoot' is called
// Specialized behavior is expected to be scripted into child classes of GunComponent
//##################################################################################################
public class GunComponent : MonoBehaviour {
    public const float GUN_MAX_RANGE = 1000.0f;

    public const bool BULLET_FIRED = true;
    public const bool BULLET_NOT_FIRED = false;

    private static PooledGameObjectManager pooledManager;

    [HeaderAttribute("Gun Component Data")]
    public GunData currentGunData;
    public Transform muzzleTransform;
    public ParticleSystem optionalCasingParticle;

    protected bool reloading = false;

    // For viewing ammo count in editor
    [SerializeField]
    protected int currentAmmoCount = 0;

    protected Timer gunTimer;
    protected Timer reloadTimer;

    protected FirstPersonPlayerComponent player;

    protected int reloadSoundId;

    //##############################################################################################
    // Check for required data, then setup the gun
    //##############################################################################################
	protected void Start(){
        if(currentGunData == null){
            Debug.LogError("Gun Data on " + gameObject.name + "'s GunComponent cannot be null");
        }

        if(muzzleTransform == null){
            Debug.LogError("Muzzle Actor on " + gameObject.name + "'s GunComponent cannot be null");
        }

        gunTimer = new Timer(currentGunData.coolDown);
        reloadTimer = new Timer(currentGunData.reloadTime);

        if(currentGunData.useAmmo){
            currentAmmoCount = currentGunData.ammoCount;
        }

        player = GetComponent<FirstPersonPlayerComponent>();

        if(currentGunData.usePooledBullets){
            if(!PooledGameObjectManager.HasPoolForIdentifier(currentGunData.poolIdentifier)){
                PooledGameObjectManager.SetupPool(currentGunData.poolIdentifier, currentGunData.poolSize, currentGunData.bulletPrefab);
            }
        }

	}

    //##############################################################################################
    // Update the reloading state
    //##############################################################################################
    protected void Update(){
        if(currentGunData.useAmmo && reloading){
            // If we're doing a progressive reload, add one ammo per timer finished
            if(currentGunData.manualReload && currentGunData.progressiveReloadInterruption){
                if(reloadTimer.Finished()){
                    reloadTimer.Start();

                    currentAmmoCount++;
                    if(currentAmmoCount ==  currentGunData.ammoCount){
                        reloading = false;
                    }
                }
            // If it's just one big reload, wait for that.
            } else {
                if(reloadTimer.Finished()){
                    reloading = false;
                    currentAmmoCount = currentGunData.ammoCount;
                }
            }
        }
    }

    //##############################################################################################
    // The simple version of shoot with no arguments, assuming it uses the gun's damage
    //##############################################################################################
    public bool Shoot(){
        return Shoot(currentGunData.damage);
    }

    //##############################################################################################
    // Create the bullet(s) and send them shooting in the direction of muzzleTransform. Also spawn
    // effects if available.
    // The argument can be used to change the damage amount (usually called from subclasses to
    // modify damage)
    // return value indicates whether or not the gun actually fired.
    //##############################################################################################
    public bool Shoot(float damage){
        if(gunTimer.Finished() && !reloading){
            gunTimer.Start();

            currentAmmoCount--;
            if(currentAmmoCount == 0){
                ReloadGun();
            }

            // This is for shotgun-type weapons. It spawns several bullets in a random cone
            for(int i = 0; i < currentGunData.shots; ++i){

                GameObject bulletInstance = null;

                if(currentGunData.usePooledBullets){
                    bulletInstance = PooledGameObjectManager.GetInstanceFromPool(currentGunData.poolIdentifier);
                } else {
                    bulletInstance = GameObject.Instantiate(currentGunData.bulletPrefab);
                }

                BulletComponent bullet = bulletInstance.GetComponent<BulletComponent>();

                if(currentGunData.usePooledBullets){
                    bullet.SetAsPooled(currentGunData.poolIdentifier);
                }

                if(bullet == null){
                    Debug.LogError("Bullet Prefab " + currentGunData.bulletPrefab.name + " must have a bullet component");
                    return BULLET_NOT_FIRED;
                }

                // Apply non-zero spread. This can be for shotgun scatter,
                // or for inaccurate, normal guns
                Quaternion spreadOffset = Quaternion.identity;
                if(currentGunData.spread > 0.0f){
                    spreadOffset = Quaternion.AngleAxis(Random.value * currentGunData.spread, Vector3.right);
                    Quaternion rot = Quaternion.AngleAxis(Random.value * 360.0f, Vector3.forward);
                    spreadOffset = rot * spreadOffset;
                }

                Vector3 bulletVelocity = Vector3.zero;

                bulletInstance.transform.position = muzzleTransform.position + muzzleTransform.TransformDirection(currentGunData.muzzleOffset);
                bulletInstance.transform.rotation = muzzleTransform.rotation * spreadOffset;

                bulletVelocity = bulletInstance.transform.forward * currentGunData.muzzleVelocity;

                // Add in player velocity if necessary
                if(player != null){
                    bulletVelocity += player.GetVelocity();
                }

                // Notify the bullet it's been fired
                bullet.Fire(damage, currentGunData.damageType, bulletVelocity, gameObject);
            }

            // Spawn effects if available, outside the loop, so there's only ever one
            if(currentGunData.firingEffectsPrefab != null){
                GameObject effectsInstance = GameObject.Instantiate(currentGunData.firingEffectsPrefab);
                effectsInstance.transform.parent = muzzleTransform;
                effectsInstance.transform.localPosition = currentGunData.firingEffectsOffset;
            }

            // If there's a casing particle, emit 1
            if(optionalCasingParticle != null){
                optionalCasingParticle.Emit(1);
            }

            // Play firing sound if it exists, outside the loop, so there's only ever one
            if(currentGunData.fireSound != null){
                SoundManagerComponent.PlaySound(
                    currentGunData.fireSound,
                    SoundCount.Single,
                    SoundType.ThreeDimensional,
                    SoundPriority.Medium,
                    currentGunData.fireSoundVolume,
                    currentGunData.fireSoundPitchBend,
                    muzzleTransform.gameObject
                );
            }

            return BULLET_FIRED;
        } else {
            return BULLET_NOT_FIRED;
        }
    }

    //##############################################################################################
    // Used to reload the gun, whether manually in a subclass, or from running out of bullets
    //##############################################################################################
    protected void ReloadGun(){
        reloading = true;
        reloadTimer.Start();

        // Play reloading sound if it exists
        if(currentGunData.reloadSound != null){
            reloadSoundId = SoundManagerComponent.PlaySound(
                currentGunData.reloadSound,
                SoundCount.Single,
                SoundType.ThreeDimensional,
                SoundPriority.Medium,
                currentGunData.reloadSoundVolume,
                currentGunData.reloadSoundPitchBend,
                muzzleTransform.gameObject
            );
        }
    }

    //##############################################################################################
    // Used by the player to impart recoil to their velocity
    //##############################################################################################
    public float GetMomentumRecoil(){
        return currentGunData.momentumRecoil;
    }

    //##############################################################################################
    // Used by the player to impart recoil to their camera look direction
    //##############################################################################################
    public float GetAimRecoil(){
        return currentGunData.aimRecoil;
    }

    //##############################################################################################
    // Getter for the current gun's muzzle velocity
    //##############################################################################################
    public float GetMuzzleVelocity(){
        return currentGunData.muzzleVelocity;
    }

    //##############################################################################################
    // Getter for the current gun's cooldown
    //##############################################################################################
    public float GetCooldown(){
        return currentGunData.coolDown;
    }
}
