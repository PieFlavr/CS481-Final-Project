using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerControl : MonoBehaviour
{
	//Player Movement Variables
	private UnityEngine.Vector2 moveAmount;
	private UnityEngine.Vector2 lastMove;
	private Rigidbody2D body;

	public float speed = 5f;
	public float dashSpeed = 25f;
	public float dashDuration = 0;
	public float dashCooldown = 0;
	public bool isFacingRight = true;

	private void Start()
	{
		body = GetComponent<Rigidbody2D>();
		//In case we want to dash while not moving
		//lastMove = new UnityEngine.Vector2(0, 0);
		//attackOrigin.position = new UnityEngine.Vector2(1, 0);
	}

	private void Update()
	{
		if (dashDuration <= 0)
        {
			body.linearVelocity = moveAmount * speed;
        }

		if (body.linearVelocityX > 0 && !isFacingRight)
		{
			Flip(2);
		}
		else if (body.linearVelocityX < 0 && isFacingRight)
		{
			Flip(0);
		}

		/*if (body.linearVelocityX > 0)
        {
			attackOrigin.position = new UnityEngine.Vector2(1, 0);
        } else if (body.linearVelocityX < 0)
        {
            
        } else
        {
            
        }*/
	}

	public void Walk(InputAction.CallbackContext walkContext)
	{
		moveAmount = walkContext.ReadValue<UnityEngine.Vector2>();

		/*In case we want to dash while not moving*/
		//UnityEngine.Vector2 val = walkContext.ReadValue<UnityEngine.Vector2>();
		//moveAmount = val;
		//if (val != new UnityEngine.Vector2(0,0))
		//{
            //lastMove = walkContext.ReadValue<UnityEngine.Vector2>();
        //}*/
	}

	public void Flip(int direction)
	{
		body.transform.localScale = new UnityEngine.Vector2(direction - 1, 1);
		isFacingRight = !isFacingRight;
	}

	public void Dash(InputAction.CallbackContext context)
	{
		// Prevent dash when clicking on UI
		if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
		{
			return;
		}

		if (context.performed == true && moveAmount != new UnityEngine.Vector2(0,0))
		{
			if (dashDuration <= 0 && dashCooldown <= 0)
            {
                dashDuration = 0.3f;
				dashCooldown = 0.25f;
				body.linearVelocity = moveAmount * dashSpeed;
				/* In case we want to dash while not moving
				if (moveAmount == new UnityEngine.Vector2(0, 0))
				{
					body.linearVelocity = lastMove * dashSpeed;
				}
				else
				{
					body.linearVelocity = moveAmount * dashSpeed;
				}*/
				StartCoroutine("stopDash");
            }
		}
	}

	public IEnumerator stopDash()
	{
		yield return new WaitForSeconds(dashDuration);
		dashDuration = 0;
		yield return new WaitForSeconds(dashCooldown);
		dashCooldown = 0;
	}
}