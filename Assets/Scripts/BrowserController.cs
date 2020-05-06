﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;

public class BrowserController : MonoBehaviour {

    const string GOOGLE_SEARCH = "https://www.google.com/search?q=";
    const string IP_ADDRESS = "192.168.0.101";
    const string IMAGE = ":3000/getImage";
    const string LINKS = ":12001";
    string uri_links;
    string uri_image;

    private void Start() {
        uri_image = "http://" + IP_ADDRESS + IMAGE;
        uri_links = "http://" + IP_ADDRESS + LINKS;
        VisitGoogle();
    }

    public void ProcessQuery(string query) {
        StopAllCoroutines();
        query = query.ToLower();
        //start loading page
        if (query.Equals("google") || query.Equals("home") || query.Equals("search")) {
            VisitGoogle();
        } else {
            SearchGoogle(query);
        }
    }

    void VisitGoogle() {
        StartCoroutine(GetImageFromURL("http://www.google.com/", true,string.Empty));
    }

    void SearchGoogle(string query) {
        string currUrl = GOOGLE_SEARCH + query;
        StartCoroutine(GetImageFromURL(currUrl, true, query));
    }

    IEnumerator GetLinksFromQuery(string query) {
        yield return new WaitForEndOfFrame();
        UnityWebRequest www = UnityWebRequest.Get(uri_links);
        www.SetRequestHeader("head", UnityWebRequest.EscapeURL(query));
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            string[] links = www.downloadHandler.text.Split(',');
            List<string> linkList = links.Distinct().ToList();
            //for whatever reason we always get this link empty so lets add the query
            string urban = "www.urbandictionary.com/define.php";
            if (linkList.Contains(urban)){
                int index = linkList.IndexOf(urban);
                linkList[index] += "?term=" + query;
            }
            foreach (string link in linkList) {
                if (link.Length > 0) {
                    print(link);
                    yield return StartCoroutine(GetImageFromURL(link, false, string.Empty));
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    /// <summary>
    /// Gets image from node.js server left here incase I want to go back to this later!
    /// </summary>
    /// <returns>The image from URL.</returns>
    IEnumerator GetImageFromURL(string url,bool isMain, string query) {
        UnityWebRequest www = UnityWebRequest.Get(uri_image);
        www.SetRequestHeader("head", url);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            byte[] imageBytes = www.downloadHandler.data;
            if (isMain) {
                SceneController.Instance.LoadMainTexture(imageBytes);
                if (query != String.Empty)StartCoroutine(GetLinksFromQuery(query));
            } else {
                SceneController.Instance.LoadLink(imageBytes);
            }
        }
    }
}