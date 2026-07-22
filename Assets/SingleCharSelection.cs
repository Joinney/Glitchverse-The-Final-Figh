using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SingleCharSelection : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public Sprite portraitSprite;
        public Sprite[] fightIdleSprites;
    }

    [Header("UI Previews (Portraits)")]
    public Image p1PreviewImage;
    public Image p2PreviewImage;
    public GameObject startBattleButton;

    [Header("UI Models (Animations)")]
    public Image p1ModelAnimImage;
    public Image p2ModelAnimImage;

    [Header("Characters List")]
    public CharacterData[] characters;

    private int p1SelectedIndex = -1;
    private int p2SelectedIndex = -1;
    private bool isP1Locked = false;
    private bool isP2Locked = false;

    private Coroutine p1AnimCoroutine;
    private Coroutine p2AnimCoroutine;

    void Start()
    {
        SetImageAlpha(p1PreviewImage, 0f);
        SetImageAlpha(p2PreviewImage, 0f);
        SetImageAlpha(p1ModelAnimImage, 0f);
        SetImageAlpha(p2ModelAnimImage, 0f);
        if (startBattleButton != null) startBattleButton.SetActive(false);
    }

    public void HoverCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        if (!isP1Locked && p1SelectedIndex == -1)
        {
            p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p1PreviewImage, 0.6f);

            if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
            p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 0.6f));
        }
        else if (isP1Locked && !isP2Locked && p2SelectedIndex == -1)
        {
            p2PreviewImage.sprite = characters[characterIndex].portraitSprite;
            SetImageAlpha(p2PreviewImage, 0.6f);

            if (p2AnimCoroutine != null) StopCoroutine(p2AnimCoroutine);
            p2AnimCoroutine = StartCoroutine(PlayModelAnimation(p2ModelAnimImage, characters[characterIndex].fightIdleSprites, 0.6f));
        }
    }

    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length) return;

        if (!isP1Locked)
        {
            if (p1SelectedIndex != characterIndex)
            {
                p1SelectedIndex = characterIndex;
                p1PreviewImage.sprite = characters[characterIndex].portraitSprite;
                SetImageAlpha(p1PreviewImage, 1f);

                if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
                p1AnimCoroutine = StartCoroutine(PlayModelAnimation(p1ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));
            }
            else
            {
                isP1Locked = true;
                PlayerPrefs.SetString("P1_Selection", characters[characterIndex].characterName);
            }
            return;
        }

        if (!isP2Locked)
        {
            if (p2SelectedIndex != characterIndex)
            {
                p2SelectedIndex = characterIndex;
                p2PreviewImage.sprite = characters[characterIndex].portraitSprite;
                SetImageAlpha(p2PreviewImage, 1f);

                if (p2AnimCoroutine != null) StopCoroutine(p2AnimCoroutine);
                p2AnimCoroutine = StartCoroutine(PlayModelAnimation(p2ModelAnimImage, characters[characterIndex].fightIdleSprites, 1f));

                if (startBattleButton != null) startBattleButton.SetActive(false);
            }
            else
            {
                isP2Locked = true;
                PlayerPrefs.SetString("P2_Selection", characters[characterIndex].characterName);

                if (startBattleButton != null) startBattleButton.SetActive(true);
            }
        }
    }

    private IEnumerator PlayModelAnimation(Image targetImage, Sprite[] sprites, float alpha)
    {
        if (targetImage == null || sprites == null || sprites.Length == 0) yield break;

        SetImageAlpha(targetImage, alpha);
        int frameIndex = 0;

        while (true)
        {
            targetImage.sprite = sprites[frameIndex];
            frameIndex = (frameIndex + 1) % sprites.Length;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void CancelSelection()
    {
        if (isP2Locked)
        {
            isP2Locked = false;
            if (startBattleButton != null) startBattleButton.SetActive(false);
        }
        else if (p2SelectedIndex != -1)
        {
            p2SelectedIndex = -1;
            SetImageAlpha(p2PreviewImage, 0f);
            SetImageAlpha(p2ModelAnimImage, 0f);
            if (p2AnimCoroutine != null) StopCoroutine(p2AnimCoroutine);

            isP1Locked = false;
        }
        else if (p1SelectedIndex != -1)
        {
            p1SelectedIndex = -1;
            SetImageAlpha(p1PreviewImage, 0f);
            SetImageAlpha(p1ModelAnimImage, 0f);
            if (p1AnimCoroutine != null) StopCoroutine(p1AnimCoroutine);
        }
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    public void LoadBattleScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}