// using System.Collections;
// using Unity.Mathematics;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.EventSystems;

// public class PlayerAttack : MonoBehaviour
// {
//     public Transform attackOrigin;
//     public float attackRadius = 1f;
//     public LayerMask enemyLayer;
//     public float attackDamage = 10;
//     public float coolDownTimer = 0;
//     public int health = 25;
//     private Rigidbody2D player;
//     public Animator meleeAnimation;

//     private void OnDrawGizmos()
//     {
//         Gizmos.DrawWireSphere(attackOrigin.position, attackRadius);
//     }

//     private void Start()
//     {
//         player = GetComponent<Rigidbody2D>();
//     }

//     private void Update()
//     {

//         if (player.linearVelocityY > 0)
//         {
//             if (player.linearVelocityY > 0 && player.linearVelocityX > 0)
//             {
//                 attackOrigin.transform.position = new UnityEngine.Vector3(player.transform.position.x + 0.55f, player.transform.position.y + 0.55f, 0);
//                 meleeAnimation.SetInteger("Direction", 4);
//                 //Debug.Log(attackOrigin.transform.position);
//             }
//             else if (player.linearVelocityY > 0 && player.linearVelocityX < 0)
//             {
//                 attackOrigin.transform.position = new UnityEngine.Vector3(player.transform.position.x - 0.55f, player.transform.position.y + 0.55f, 0);
//                 meleeAnimation.SetInteger("Direction", 4);
//             }
//             else
//             {
//                 attackOrigin.transform.position = new UnityEngine.Vector3(player.transform.position.x, player.transform.position.y + 0.7f, 0);
//                 meleeAnimation.SetInteger("Direction", 2);
//             }
//         }
//         else if (player.linearVelocityY < 0)
//         {
//             if (player.linearVelocityY < 0 && player.linearVelocityX > 0)
//             {
//                 attackOrigin.transform.position = new UnityEngine.Vector3(player.transform.position.x + 0.55f, player.transform.position.y - 0.55f, 0);
//                 meleeAnimation.SetInteger("Direction", 3);
//             }
//             else if (player.linearVelocityY < 0 && player.linearVelocityX < 0)
//             {
//                 attackOrigin.transform.position = new UnityEngine.Vector3(player.transform.position.x - 0.55f, player.transform.position.y - 0.55f, 0);
//                 meleeAnimation.SetInteger("Direction", 3);
//             }
//             else
//             {
//                 attackOrigin.transform.position = new UnityEngine.Vector3(player.transform.position.x, player.transform.position.y - 0.7f, 0);
//                 meleeAnimation.SetInteger("Direction", 1);
//             }
//         }
//         else if (player.linearVelocityX > 0)
//         {
//             attackOrigin.transform.position = new UnityEngine.Vector3(player.transform.position.x + 0.7f, player.transform.position.y, 0);
//             meleeAnimation.SetInteger("Direction", 0);
//         }
//         else if (player.linearVelocityX < 0)
//         {
//             attackOrigin.transform.position = new UnityEngine.Vector3(player.transform.position.x - 0.7f, player.transform.position.y, 0);
//             meleeAnimation.SetInteger("Direction", 0);
//         }
//     }

//     public void Melee(InputAction.CallbackContext context)
//     {
//         // Prevent melee attack when clicking on UI
//         if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
//         {
//             Debug.Log("PlayerAttack: Click blocked - pointer over UI");
//             return;
//         }

//         if (context.performed == true)
//         {
//             if (coolDownTimer == 0)
//             {
//                 meleeAnimation.SetBool("Attacking", true);
//                 coolDownTimer = 0.5f;
//                 Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, enemyLayer);
//                 foreach (var enemy in enemiesInRange)
//                 {
//                     // enemy.GetComponent<HealthManager>().TakeDamage((int)attackDamage);
//                     //Debug.Log("Fudge Hit");
//                 }

//                 StartCoroutine("coolDown");
//             }
//             else
//             {
//                 //Debug.Log("Fudge No Hit" + coolDownTimer);
//             }
//             //Debug.Log("f");
//         }
//         //Debug.Log("Fudge");
//     }

//     public void finishAttack()
//     {
//         meleeAnimation.SetBool("Attacking", false);
//     }
    
//     public IEnumerator coolDown()
//     {
//         yield return new WaitForSeconds(coolDownTimer);
//         coolDownTimer = 0;
//     }
// }
