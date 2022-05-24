using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI {
    public class OfferShowcasePresenter : Presenter<OfferShowcasePresenter.Model> {
        [Serializable]
        public class Model : Presenter.Model {
            public List<string> URLs = new List<string>();
            public string VideoURL;

            Texture2D m_variantTexture = null;
            List<Texture2D> m_Textures = new List<Texture2D>();
            public Texture2D GetTexture(int index)
            {
                if(index<m_Textures.Count)
                {
                    return m_Textures[index];
                }
                return null;
            }

            public int TextureCount()
            {
                return m_Textures.Count;
            }

            public void AddTexture(Texture2D tex)
            {
                m_Textures.Add(tex);
            }

            public override void Dispose()
            {
                base.Dispose();
                ClearVariantTexture();
                // Destroy the textures from the list and empty the display 
                while (m_Textures.Count != 0)
                {
                    if (m_Textures[0])
                        Destroy(m_Textures[0]);
                    m_Textures.RemoveAt(0);
                }
            }

            public void ClearVariantTexture()
            {
                if (m_variantTexture) Destroy(m_variantTexture);
                m_variantTexture = null;
            }

            public void AddVariantTexture(Texture2D tex)
            {
                ClearVariantTexture();
                m_variantTexture = tex;
            }
        }

        [SerializeField] ZoomOfferDetails panningAndPinchImage;
        [SerializeField] Image imageDisplay;
        [SerializeField] GameObject thumbnailPrefab;
        [SerializeField] Transform thumbnailParent;

        private List<string> m_Urls = new List<string>();

        List<GameObject> m_Thumbs = new List<GameObject>();
        int m_Index;

        protected override void Awake()
        {
            base.Awake();
            imageDisplay.preserveAspect = true;
        }
        
        public void Next() {
            m_Index++;
            RefreshDisplay();
        }

        void RefreshDisplay() {
            Debug.Log("Refresing IMAGE");

            if (m_Index == Data.URLs.Count)
                m_Index = 0;

            if (Data.TextureCount() > m_Index) 
            {
                Debug.Log("DIPOLSAY IMAGEN --------");
                imageDisplay.sprite = Data.GetTexture(m_Index).ToSprite();
            }
                
        }

        void RefreshDisplayReduced()
        {
            if (m_Index == m_Urls.Count)
                m_Index = 0;

            Data.Dispose();
            Debug.Log("New index image: " + m_Index);
            DownloadLargeImagen(m_Index);

        }
             
        
        protected override void OnSetState()
        {
            ResetSizeImage();
            Clear();
            DownloadImages();
        }

        public void Clear() {
            m_Index = 0;

            imageDisplay.sprite = null;

            // Remove thumbnails from the list and the instantiated buttons
            while (m_Thumbs.Count > 0) {
                if(m_Thumbs[0])
                    Destroy(m_Thumbs[0]);
                m_Thumbs.RemoveAt(0);
            }
            foreach (Transform thumb in thumbnailParent)
                Destroy(thumb.gameObject);
        }

        public void ResetSizeImage()
        {
            //panningAndPinchImage.ResetSizeImage();
        }

        async void DownloadImages()
        {
            var downloader = ImageDownloader.New(false);
            var downloads = new List<Task<Texture2D>>();

            int amountLimitImages = 6;
            int counterLargeImage = 0;
            int indexLarge = -1;

            Debug.Log("Count URLs 1: " + Data.URLs.Count);

            if (Data.URLs.Count < amountLimitImages)
                amountLimitImages = Data.URLs.Count;

            m_Urls = new List<string>(Data.URLs.GetRange(0, amountLimitImages));

            List<string> newListUrl = new List<string>(m_Urls);

            // Convert the urls to medium for thumbs
            for (int i = 0; i < newListUrl.Count; i++)
            {
                if (!String.IsNullOrEmpty(newListUrl[i]) && newListUrl[i].IndexOf("shopify") != -1 && newListUrl[i].LastIndexOf(".") != -1)
                {
                    Debug.Log("***** Images loaded from SHOPIFY: "+i);
                    newListUrl[i] = newListUrl[i].Insert(newListUrl[i].LastIndexOf("."), "_medium");
                    counterLargeImage++;
                    if(counterLargeImage > 0 && indexLarge < 0)
                    {
                        indexLarge = i;
                    }
                }
            }
            int counterImages = 0;


            for (int i = 0; i < newListUrl.Count; i++)
            {
                Debug.Log("URL: " + i + " " + newListUrl[i]);
            }


            newListUrl.ForEach(x => downloads.Add(downloader.Download(x)));
            thumbnailParent.gameObject.SetActive(false);
            // Downloads the iamges together and process as they finish         
            while (downloads.Count > 0)
            {
                try
                {
                    var finished = await Task.WhenAny(downloads);
                    if (finished.Exception == null)
                    {
                        downloads.Remove(finished);
                        if (finished.Result != null)
                        {

                            //AddDisplayImage(finished.Result); 
                            AddThumbnail(finished.Result);
                            // Hack, add scrollable area instead
                            //if (Data.TextureCount() > 3) break;
                        }
                    }
                    else
                    {
                        downloads.Remove(finished);
                    }

                }
                catch (Exception e)
                {
                    ScutiLogger.LogException(e);
                }

                counterImages++;
            }

            Destroy(downloader.gameObject);

            DownloadLargeImagen(indexLarge);
        }


        async void DownloadLargeImagen(int indexLarge)
        {
            var downloader = ImageDownloader.New(false);
            var downloads = new List<Task<Texture2D>>();

            //int amountLimitImages = m_Urls.Count;

            //if (Data.URLs.Count < amountLimitImages)
            //    amountLimitImages = Data.URLs.Count;

            Debug.Log("Count URLs 2: " + m_Urls.Count);
            //m_Urls = new List<string>(Data.URLs.GetRange(0, amountLimitImages));
            Debug.Log("Index large image: " + indexLarge);
            List<string> newListUrl = new List<string>(m_Urls);

            for(int i = 0; i < newListUrl.Count; i++)
            {
                Debug.Log("URL: " + i + " " + newListUrl[i]);
            }


            if (newListUrl.Count > 0)
            {
                if (indexLarge >= 0)
                {
                    if (!String.IsNullOrEmpty(newListUrl[indexLarge]) && newListUrl[indexLarge].IndexOf("shopify") != -1 && newListUrl[indexLarge].LastIndexOf(".") != -1)
                    {
                        // if source its shopify
                        newListUrl[indexLarge] = newListUrl[indexLarge].Insert(newListUrl[indexLarge].LastIndexOf("."), "_1024x1024");
                        downloads.Add(downloader.Download(newListUrl[indexLarge]));
                        Debug.Log("LargeImage downloaded 1: " + newListUrl[indexLarge]);
                    }
                    else 
                    {
                        downloads.Add(downloader.Download(newListUrl[indexLarge]));
                        Debug.Log("LargeImage downloaded 2: " + newListUrl[indexLarge]);
                    }
                }
                else
                {
                    downloads.Add(downloader.Download(newListUrl[0]));
                    Debug.Log("LargeImage downloaded 0: " + newListUrl[0]);
                }
            }           

            try
            {
                var finished = await Task.WhenAny(downloads);
                if (finished.Exception == null)
                {                    
                    if (finished.Result != null)
                    {
                        AddDisplayImage(finished.Result); 
                    }
                    downloads.Remove(finished);
                }
                else
                {
                    downloads.Remove(finished);
                }

            }
            catch (Exception e)
            {
                ScutiLogger.LogException(e);
            }                
            
            Destroy(downloader.gameObject);
        }


        void AddDisplayImage(Texture2D texture){

            Data.AddTexture(texture);
            Debug.Log("ADD DISPLAY IMAGE: "+Data.TextureCount());
            // If this is the first texture, set it to display
            if (Data.TextureCount() == 1) {
                imageDisplay.sprite = Data.GetTexture(0).ToSprite();
                m_Index = 0;
            }
        }

        void AddThumbnail(Texture2D texture2D) {

            var instance = Instantiate(thumbnailPrefab, thumbnailParent);
            instance.hideFlags = HideFlags.DontSave;

            var index = m_Thumbs.Count;
            m_Thumbs.Add(instance);

            if(m_Thumbs.Count>1)
                thumbnailParent.gameObject.SetActive(true);
            var image = instance.GetComponent<Image>();
            image.sprite = texture2D.ToSprite();
            var button = instance.GetComponent<Button>();
            button.onClick.AddListener(() => {
                m_Index = index;
                RefreshDisplayReduced();
                //RefreshDisplay();
            });
        }

        internal void SetVariant(Scuti.GraphQL.Generated.ProductVariant productVariant)
        {
            //Data.ClearVariantTexture();
            //RefreshDisplay();
            if (productVariant!=null)
            {
                if(!string.IsNullOrEmpty(productVariant.Image))
                {
                    Debug.Log("Set VARIANT LOAD IMAGE");
                    LoadVariantImage(productVariant.Image);
                }
            }
        }

        async void LoadVariantImage(string image)
        {
            var downloader = ImageDownloader.New(true);

            if (!String.IsNullOrEmpty(image) && image.IndexOf("shopify") != -1 && image.LastIndexOf(".") != -1)
            {
                image = image.Insert(image.LastIndexOf("."), "_1024x1024");
            }

            var tex = await downloader.Download(image);
            Data.ClearVariantTexture();
            Data.AddVariantTexture(tex);
            Debug.Log("DIPOLSAY IMAGEN 2 --------");
            imageDisplay.sprite = tex.ToSprite();
        }
    }
}
