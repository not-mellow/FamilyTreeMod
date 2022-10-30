using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;

namespace FamilyTreeMod
{
    class FamilyWindow : MonoBehaviour
    {
        public static Dictionary<int, List<GameObject>> activeGameObjects = new Dictionary<int, List<GameObject>>();
        public static FamilyWindow familyWindowInstance;
        private static float loadingValue = 0f;
        private static float maxValue = 0f;
        private static GameObject loadingBar;
        private static GameObject inspectLoadingBar;
        private static GameObject zoomHolder;
        private static GameObject relationsButton;
        private static bool familyLoaded = false;
        public static float loadTime = 0.2f;

        public static void init()
        {
            var scrollView = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyWindow/Background/Scroll View");
            var scrollGradient = GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/familyWindow/Background/Scrollgradient");
            scrollGradient.gameObject.SetActive(true);

            RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
            scrollViewRect.sizeDelta = new Vector2((float)200, (float)215.21);
            scrollViewRect.localPosition = new Vector3(0, 0, 0);

            ScrollRect scrollRect = scrollView.gameObject.GetComponent<ScrollRect>();
            scrollRect.horizontal = true;
            
            var scrollViewInspect = GameObject.Find($"/Canvas Container Main/Canvas - Windows/windows/familyInspectWindow/Background/Scroll View");
            var scrollGradientInspect = GameObject.Find($"Canvas Container Main/Canvas - Windows/windows/familyInspectWindow/Background/Scrollgradient");
            scrollGradientInspect.gameObject.SetActive(true);

            RectTransform scrollViewInspectRect = scrollViewInspect.GetComponent<RectTransform>();
            scrollViewInspectRect.sizeDelta = new Vector2((float)200, (float)215.21);
            scrollViewInspectRect.localPosition = new Vector3(0, 0, 0);

            ScrollRect scrollInspectRect = scrollViewInspect.gameObject.GetComponent<ScrollRect>();
            scrollInspectRect.horizontal = true;

            GameObject newInstance = new GameObject("familyWindowInstance");
            FamilyWindow familyWindow = newInstance.AddComponent<FamilyWindow>();
            familyWindowInstance = familyWindow;

            zoomHolder = NewActions.createZoomButton(scrollView);
            zoomHolder.SetActive(false);

            loadingBar = NewActions.createProgressBar(scrollView);
            loadingBar.SetActive(false);

            inspectLoadingBar = NewActions.createProgressBar(scrollViewInspect);
            inspectLoadingBar.SetActive(false);
        }

        public static void showFamily(ActorParent actorParent, bool searching)
        {
            activeGameObjects.Clear();
            loadTime = 0.3f;
            if (searching)
            {
                var inspectRect = WindowManager.windowContents["familyInspectWindow"].GetComponent<RectTransform>();
                inspectRect.sizeDelta = new Vector2(0, 150);

                foreach (Transform child in  WindowManager.windowContents["familyInspectWindow"].transform)
                {
                    Destroy(child.gameObject);
                }

                GameObject parent = addAvatarSlot(
                actorParent.parentActor.getName(),
                WindowManager.windowContents["familyInspectWindow"],
                new Vector3((inspectRect.rect.width/2), (-inspectRect.rect.height/2), 0),
                actorParent.parentActor
                );

                loadingValue = 0f;
                maxValue = 0f;
                inspectLoadingBar.SetActive(true);

                familyWindowInstance.callSearchFamily(parent, 1, actorParent, 0);
                if (actorParent.children.Count > 0 /*|| actorParent.childrenData.Count > 0*/)
                {
                    familyWindowInstance.callMakeFamily(parent, new Vector3((inspectRect.rect.width/2), 0, 0), actorParent, 80, WindowManager.windowContents["familyInspectWindow"], 0);
                    RectTransform parentRect = parent.GetComponent<RectTransform>();
                    parentRect.localPosition = new Vector3(inspectRect.rect.width/2, -inspectRect.rect.height/2, 0);
                }
                else if(loadingValue == maxValue)
                {
                    inspectLoadingBar.SetActive(false);
                }
                inspectRect.localPosition = new Vector3(-inspectRect.rect.width/2, inspectRect.rect.height/2, 0);
                Windows.ShowWindow("familyInspectWindow");
            }
            else
            {
                var contentRect = WindowManager.windowContents["familyWindow"].GetComponent<RectTransform>();
                contentRect.sizeDelta = new Vector2(0, 150);

                foreach (Transform child in  WindowManager.windowContents["familyWindow"].transform)
                {
                    Destroy(child.gameObject);
                }

                zoomHolder.SetActive(true);
                familyLoaded = false;

                GameObject parent = addAvatarSlot(
                actorParent.parentActor.getName() + " Family",
                WindowManager.windowContents["familyWindow"],
                new Vector3((contentRect.rect.width/2), -40, 0),
                actorParent.parentActor
                );
                if (actorParent.children.Count > 0 /*|| actorParent.childrenData.Count > 0*/)
                {
                    loadingValue = 0f;
                    maxValue = 0f;
                    loadingBar.SetActive(true);

                    familyWindowInstance.callMakeFamily(parent, new Vector3((contentRect.rect.width/2), -40, 0), actorParent, 80, WindowManager.windowContents["familyWindow"], 0);
                    RectTransform parentRect = parent.GetComponent<RectTransform>();
                    parentRect.localPosition = new Vector3(contentRect.rect.width/2, -40, 0);
                }
                else
                {
                    familyLoaded = true;
                }
                Windows.ShowWindow("familyWindow");
            }
        }

        public void callSearchFamily(GameObject parent, int Ypos, ActorParent actorParent, int bgHolderIndex)
        {
            StartCoroutine(searchFamilyTree(parent, Ypos, actorParent, bgHolderIndex));
        }

        public void callMakeFamily(GameObject parent, Vector3 parentPos, ActorParent chosenFamily, int contentSizeX, GameObject window, int bgHolderIndex)
        {
            StartCoroutine(makeFamilyTree(parent, parentPos, chosenFamily, contentSizeX, window, bgHolderIndex));
            if (maxValue % 500 == 0 && maxValue != 0 && loadTime <= 6f)
            {
                loadTime += 0.2f;
            }
        }

        public static GameObject addAvatarSlot(string textString, GameObject canvasParent, Vector3 pos, Actor actor)
        {
            GameObject avatarSlot = NewActions.addCanvasRenderer(canvasParent, pos);
            NewActions.addText(textString, avatarSlot, 16);
            NewActions.addUnitAvatar(actor, avatarSlot);

            return avatarSlot;
        }

        public static IEnumerator searchFamilyTree(GameObject parent, int Ypos, ActorParent actorParent, int bgHolderIndex)
        {
            maxValue += 1f;
            RectTransform contentRect = WindowManager.windowContents["familyInspectWindow"].GetComponent<RectTransform>();
            bgHolderIndex++;

            if (actorParent.parentFamily != actorParent)
            {
                GameObject ancestor = addAvatarSlot(actorParent.parentFamily.parentActor.getName(), parent, new Vector3(0, Ypos*160, 0), actorParent.parentFamily.parentActor);
                yield return new WaitForSeconds(.1f);
                ancestor.transform.name += bgHolderIndex.ToString();
                Ypos++;
                familyWindowInstance.callSearchFamily(parent, Ypos, actorParent.parentFamily, bgHolderIndex);
                yield return new WaitForSeconds(loadTime);
            }
            contentRect.sizeDelta += new Vector2(0, Ypos*20);
            loadingValue += 1f;
            StatBar statBar = inspectLoadingBar.GetComponent<StatBar>();
            statBar.setBar(loadingValue, maxValue, "", false, true);
            yield break;
        }

        public static IEnumerator makeFamilyTree(GameObject parent, Vector3 parentPos, ActorParent chosenFamily, int contentSizeX, GameObject window, int bgHolderIndex)
        {
            maxValue += 1f;
            RectTransform contentRect = window.GetComponent<RectTransform>();
            bgHolderIndex++;

            int xPos = -200 * (int)(chosenFamily.children.Count/2);

            // foreach (ActorStatus status in chosenFamily.childrenData)
            // {

            // }

            foreach (Actor actor in chosenFamily.children.ToList())
            {
                GameObject newParent = addAvatarSlot(actor.getName(), parent, new Vector3(/*parentPos.x + */xPos, parentPos.y - 160, 0), actor);
                newParent.transform.name += bgHolderIndex.ToString();
                if (FamilyOverviewWindow.familyActors.Contains(actor))
                {
                    foreach (ActorParent family in FamilyOverviewWindow.families.ToList())
                    {
                        if (family.parentActor == actor)
                        {
                            familyWindowInstance.callMakeFamily(newParent, new Vector3(0, parentPos.y , 0), family, 30, window, bgHolderIndex);
                            yield return new WaitForSeconds(loadTime);
                        }
                    }
                }
                xPos += 200;
                yield return new WaitForSeconds(loadTime);
            }

            List<GameObject> childrenList = new List<GameObject>();
            // bool showButtonAdded = false;
            foreach(Transform child in parent.transform)
            {
                if (child.name.Contains("bgHolder"))
                {
                    child.gameObject.SetActive(false);
                    childrenList.Add(child.gameObject);
                    // if (!showButtonAdded) NewActions.addShowChildrenButton(parent, childrenList);
                    contentRect.sizeDelta += new Vector2(0, 10);
                    // showButtonAdded = true;
                    //var newArrow = NewActions.addArrow(parent, child.gameObject);
                    //newArrow.SetActive(false);
                    // childrenList.Add(newArrow);
                }
            }
            if (childrenList.Count > 0)
            {
                NewActions.addShowChildrenButton(parent, childrenList);
            }
            
            contentRect.sizeDelta += new Vector2(contentSizeX*chosenFamily.children.Count, 0);
            contentRect.localPosition = new Vector3(-contentRect.sizeDelta.x/2, (contentRect.sizeDelta.y/2) - 60, 0);
            loadingValue += 1f;
            StatBar statBar = loadingBar.GetComponent<StatBar>();
            statBar.setBar(loadingValue, maxValue, "", false, true);
            StatBar inspectStatBar = inspectLoadingBar.GetComponent<StatBar>();
            inspectStatBar.setBar(loadingValue, maxValue, "", false, true);
            if(loadingValue == maxValue)
            {
                loadingBar.SetActive(false);
                inspectLoadingBar.SetActive(false);
                familyLoaded = true;
            }
            yield break;
        }

        public static void zoomContent(float value)
        {
            if(!familyLoaded)
            {
                return;
            }
            RectTransform contentRect = WindowManager.windowContents["familyWindow"].GetComponent<RectTransform>();

            foreach(Transform child in WindowManager.windowContents["familyWindow"].transform)
            {
                if (child.name.Contains("bgHolder"))
                {
                    child.localScale += new Vector3(value, value, 0);
                    if (child.localScale.magnitude < new Vector3(0.1f, 0.1f, 1).magnitude)
                    {
                        child.localScale = new Vector3(0.1f, 0.1f, 1f);
                        continue;
                    }
                    if (child.localScale.magnitude > new Vector3(1, 1, 1).magnitude)
                    {
                        child.localScale = new Vector3(1f, 1f, 1f);
                        continue;
                    }
                    contentRect.localPosition = new Vector3(-contentRect.sizeDelta.x/2, (contentRect.sizeDelta.y/2) - 60, 0);
                }
            }
        }
    }
}