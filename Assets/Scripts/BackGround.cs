using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class BackGround : MonoBehaviour, IPointerClickHandler
{
    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        transform.localScale = new Vector3(1, 1, 1);

        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;
        transform.localScale = new Vector3(worldScreenWidth / width, worldScreenHeight / height, 1);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        var gameField = GameField.Instance;
        if (gameField.InAnimation) return;
        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.zero);
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("BackGround"))
        {
            if (!gameField.gameObject.activeInHierarchy)
            {
                gameField.OpenGame();
            }
            else
            {
                gameField.CloseGame();
            }
        }
    }
}