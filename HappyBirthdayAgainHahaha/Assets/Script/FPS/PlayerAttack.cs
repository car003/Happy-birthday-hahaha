using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    //bool isNeedToCD = true;
    bool canAttack = true;
    float playerAttackCD = 2f;
    float playerCurrAttackCD = 0f;
    float playerAttackPower = 20f;

    [Header("Keybinds")]
    public KeyCode attackKey = KeyCode.K;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(attackKey))
        {
            Attack();
        }
    }

    [SerializeField]int distanceOfRaycast = 2;
    RaycastHit _hit;

    void Attack()
    {
        //do CD end?
        if (!canAttack)
            return;



        //animation.....
        //special effects?....
        //Sound effects?.....


        //CD
        StartCoroutine(ResetAttackCD());

        //find Enemy
        Ray ray = new Ray(transform.position, transform.right);
        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.right) * distanceOfRaycast, Color.white);

        if (Physics.Raycast(ray, out _hit, distanceOfRaycast))
        {
            if (_hit.transform.TryGetComponent<EnemyInformation>(out EnemyInformation enemy))
            {
                enemy.SetEnemyHP(-playerAttackPower);
            }
        }
    }

    IEnumerator ResetAttackCD()
    {
        Debug.Log("ResetAttackCD()");
        canAttack = false;

        yield return new WaitForSeconds(playerAttackCD);
        //This causes the code to wait here for 3 seconds before continuing.

        canAttack = true;
    }
}
