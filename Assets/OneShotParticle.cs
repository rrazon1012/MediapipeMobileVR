using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotParticle : MonoBehaviour
{

  public ParticleSystem explosion;
  public ParticleSystem smoke;
  public ParticleSystem sparks;

  // Update is called once per frame
  void Update()
  {
    if (!explosion.IsAlive())
    {
      Destroy(explosion);
    }

    if (!smoke.IsAlive()) 
    {
      Destroy(smoke);
    }

    if (!sparks.IsAlive())
    {
      Destroy(sparks);
    }

    if(!smoke.IsAlive() && !explosion.IsAlive() && !sparks.IsAlive())
    {
      Destroy(gameObject);
    }
  }
}
