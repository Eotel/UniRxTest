using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Sample.Section1
{
    public class DownloadTexture : MonoBehaviour
    {
        // uGUI の RawImage
        [SerializeField] private RawImage _rawImage;

        void Start()
        {
            var uri = "https://cdn-ak.f.st-hatena.com/images/fotolife/h/halya_11/20190206/20190206225335.png";

            // テクスチャを取得する
            // ただし例外発生時には計３回まで試行する
            GetTextureAsync(uri)
                .OnErrorRetry(
                    onError: (Exception _) => { },
                    retryCount: 3)
                .Subscribe(
                    result => { _rawImage.texture = result; },
                    Debug.LogError
                ).AddTo(this);
        }

        /// <summary>
        /// コルーチンを起動して，その結果を Observable で返す
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private IObservable<Texture> GetTextureAsync(string uri)
        {
            return Observable.FromCoroutine<Texture>(
                observer => GetTextureCoroutine(observer, uri));
        }


        private IEnumerator GetTextureCoroutine(IObserver<Texture> observer, string uri)
        {
            using var uwr = UnityWebRequestTexture.GetTexture(uri);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                observer.OnError(new Exception(uwr.error));
                yield break;
            }

            var result = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            // 成功したら OnNext/OnCompleted メッセージを発行する
            observer.OnNext(result);
            observer.OnCompleted();
        }
    }
}