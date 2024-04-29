using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    public TextMeshProUGUI npcText;
    public string[] lines;
    public float textSpeed;
    private int index;
    // Start is called before the first frame update
    void Start()
    {
        npcText.text = string.Empty;
        StartDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            if(npcText.text == lines[index]){
                NextLine();
            }
            else{
                StopAllCoroutines();
                npcText.text = lines[index];
            }
        }
    }
    void StartDialogue(){
        index = 0;
        StartCoroutine(TypeLine());
    }
    IEnumerator TypeLine(){
        foreach(char c in lines[index].ToCharArray()){
            npcText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }
    void NextLine(){
        if(index < lines.Length - 1){
            index++;
            npcText.text = string.Empty;
            StartCoroutine(TypeLine());
        }else{
            gameObject.SetActive(false);
        }
    }
}
