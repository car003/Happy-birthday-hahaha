using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInformation : MonoBehaviour
{
    float enemyHP = 100f;
    float enemyMP = 100f;
    float enemyMoveSpeed = 5f;
    float enemyAttackCD = 2f;
    float enemyAttackPower = 10f;

    public void SetEnemyHP(float value)
    {
        enemyHP += value;

        BeAttack();
    }

    public float GetEnemyHP()
    {
        return enemyHP;
    }

    void EnemyDie()
    {
        //special effects?.....
        //Sound effects?....
        //animation?.....

        Destroy(this.gameObject);
    }

    void BeAttack()
    {
        //special effects?.....
        //Sound effects?....
        //animation?.....

        //be repel


        if (enemyHP <= 0)
            EnemyDie();
    }


}
