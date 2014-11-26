	using UnityEngine;
using System.Collections;

public class TestCharMove : MonoBehaviour {
	//Animator 컴포넌트에 명령을 전달할 때 사용.
	Animator animator;
	//키보드에 의한 좌우 움직임 크기를 저장.
	float horizontalAmount;
	//좌우 움직임 속도를 제어할 때 사용.
	Vector2 horizontalSpeed;
	
	//캐릭터의 방향이 오른쪽인지 식별.
	[HideInInspector]
	public bool facingRight = true;			
	//캐릭터의 점프 조건.
	[HideInInspector]
	public bool jump = false;				

	//최대 속도와 이동 및 점프에 가해질 힘을 정의하는데 사용.
	public float maxSpeed = 5.0f;
	public float moveForce = 365f;
	public float jumpForce = 300f;
	
	//캐릭터가 바닥에 닿았다는 것을 판단하기 위해
	//Linecast를 활용하여 체크할 때
	//Linecast가 도달하는 Position값을 가지는 Transform
	public Transform groundCheck;
	//캐릭터가 Ground레이어 오브젝트 위에 있는지 식별.
	private bool grounded = false;

	void OnEnable ()
	{
		//Animator 컴포넌트 할당.
		animator = GetComponent<Animator>();
	}

	void Flip ()
	{
		//캐릭터를 좌우 반전하였을 때 캐릭터가 어느쪽 방향을 지향하고 있는지 저장.
		facingRight = !facingRight;
		
		//캐릭터를 뒤짚는다.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	void Update()
	{
		//Ground 레이어의 어떤 오브젝트라도 Linecast에 hit된다면 true
		grounded = 
			Physics2D.Linecast(transform.position, 
			                   groundCheck.position, 
			                   1 << LayerMask.NameToLayer("Ground")); 
		
		//Jump버튼을 눌렀을 때 캐릭터가 grounded라면 점프로 식별.
		if(Input.GetButtonDown("Jump") && grounded)
		{
			jump = true;
		}
	}

	void FixedUpdate ()
	{
		horizontalAmount = Input.GetAxis("Horizontal");
		//애니메이터 컴포넌트에 매개 변수인 moveLR을 키보드 입력값의 크기를 전달하여 자연히 상태 전이가 발생하도록 한다.
		animator.SetFloat("moveLR", Mathf.Abs(horizontalAmount));
		
		if(horizontalAmount * rigidbody2D.velocity.x < maxSpeed)
		{
			//좌우로 움직인다.
			rigidbody2D.AddForce(Vector2.right * horizontalAmount * moveForce);
		}
		
		//최고 속도보다 빨라지면 최고 속도로 속도를 늦춘다.
		if(Mathf.Abs(rigidbody2D.velocity.x) > maxSpeed)
		{
			rigidbody2D.velocity = 
				new Vector2(Mathf.Sign(rigidbody2D.velocity.x) * maxSpeed, 
				            rigidbody2D.velocity.y);
		}
		
		//오른쪽 방향으로 움직이게 입력했을 때 캐릭터가 왼쪽 방향이면 캐릭터를 뒤집는다.
		if(horizontalAmount > 0 && !facingRight)
		{
			Flip();
		}
		//반대의 경우도 뒤짚는다.
		else if(horizontalAmount < 0 && facingRight)
		{
			Flip();
		}
		
		//점프가 발생하면 캐릭터를 위로 움직인다.
		if(jump)
		{
			//애니메이터에 트리거 발생.
			animator.SetTrigger("jump");
			
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));
			
			jump = false;
		}

		//키보드 입력이 없다면 캐릭터를 멈추도록 한다.
		if( horizontalAmount == 0 )
		{
			horizontalSpeed = rigidbody2D.velocity;
			horizontalSpeed.x = 0;
			rigidbody2D.velocity = horizontalSpeed;
		}
	}
}
