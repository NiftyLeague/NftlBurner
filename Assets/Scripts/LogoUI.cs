using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoUI : MonoBehaviour
{
	public SimpleAnim logoAnim;
	public SpriteRenderer logoSpriteRenderer;
	public int[] animSpeedChangeFrames;
	public float[] animSpeeds;
	public float fadeoutSpeed;

	public int logoSheenFrame;
	public int batHitFrame;
	public int SheatheFrame;
	public int logoSparkleFrame;

	public AudioSource audioSource;
	public AudioClip clip01LogoSheen;
	public AudioClip clip02BatHit;
	public AudioClip clip03Sheathe;
	public AudioClip clip04GlassSparkle;

	private int currentAnimSpeedIndex = 0;
	private bool skipPressed = false;

	private void Start()
	{
		StartCoroutine(Animate());
		StartCoroutine(FadeOut());
	}

	private void Update()
	{
		UpdateAnimationSpeed();
		if (Input.anyKey)
		{
			skipPressed = true;
		}
	}

	private void UpdateAnimationSpeed()
	{
		if (currentAnimSpeedIndex >= animSpeeds.Length)
		{
			return;
		}

		if (logoAnim.GetFrame() >= animSpeedChangeFrames[currentAnimSpeedIndex])
		{
			logoAnim.animSpeed = animSpeeds[currentAnimSpeedIndex];
			currentAnimSpeedIndex++;
		}
	}

	private IEnumerator Animate()
	{
		yield return new WaitForSecondsRealtime(0.25f);
		logoAnim.Play();

		yield return new WaitUntil(() => logoAnim.GetFrame() >= logoSheenFrame);
		audioSource.PlayOneShot(clip01LogoSheen);

		yield return new WaitUntil(() => logoAnim.GetFrame() >= batHitFrame);
		audioSource.PlayOneShot(clip02BatHit);

		yield return new WaitUntil(() => logoAnim.GetFrame() >= SheatheFrame);
		audioSource.PlayOneShot(clip03Sheathe);

		yield return new WaitUntil(() => logoAnim.GetFrame() >= logoSparkleFrame);
		audioSource.PlayOneShot(clip04GlassSparkle);
	}

	private IEnumerator FadeOut()
	{
		yield return new WaitUntil(() => logoAnim.GetProgress() >= 1f || skipPressed);
		while (logoSpriteRenderer.color.a > 0f)
		{
			Color c = logoSpriteRenderer.color;
			c.a -= Time.deltaTime * fadeoutSpeed;
			logoSpriteRenderer.color = c;
			audioSource.volume = c.a;
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(0.25f);
		SceneManager.LoadScene("Main");
	}
}
