
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    public class UITest1: MonoBehaviour
    {
        public GameObject parent;
        public Animator animator;
        public Image image;
        

        private void OnEnable()
        {
            Debug.Log("UITest1 OnEnable");
        }
        
        public void ClickAni()
        {
            // image.gameObject.SetActive(false);
            animator.SetTrigger("in");
          
            // HideParent();
            // StartCoroutine(ShowParent());
          
        }
        
        public void ClickRest()
        {
            Debug.Log("ClickAni");
            animator.SetTrigger("reset");
        }

        IEnumerator PlayInAni()
        {
            yield return new WaitForSeconds(0.5f);
            image.gameObject.SetActive(true);
            animator.SetTrigger("in");
        }
        
        public void HideAndRest()
        {
            Debug.Log("HideAndRest");
            animator.gameObject.SetActive(false);
        }

        public  void HideParent()
        {
            parent.SetActive(false);
        }
        
        private IEnumerator ShowParent()
        {
            yield return new WaitForSeconds(0.2f);
            Destroy(animator);
        }
        


    }
