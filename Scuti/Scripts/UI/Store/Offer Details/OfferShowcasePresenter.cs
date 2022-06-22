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
        [SerializeField] RectTransform thumbnailParent;

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

            if (m_Index == Data.URLs.Count)
                m_Index = 0;

            if (Data.TextureCount() > m_Index) 
                imageDisplay.sprite = Data.GetTexture(m_Index).ToSprite();
            
                
        }

        void RefreshDisplayReduced()
        {
            if (m_Index == m_Urls.Count)
                m_Index = 0;

            Data.Dispose();
            DownloadLargeImage(m_Index);

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

            int amountLimitImages = 15;
            int counterLargeImage = 0;
            int indexLarge = -1;

            if (Data.URLs.Count < amountLimitImages)
                amountLimitImages = Data.URLs.Count;

            m_Urls = new List<string>(Data.URLs.GetRange(0, amountLimitImages));

            List<string> newListUrl = new List<string>(m_Urls);

            // Convert the urls to medium for thumbs
            for (int i = 0; i < newListUrl.Count; i++)
            {
                if (!String.IsNullOrEmpty(newListUrl[i]) && newListUrl[i].IndexOf("shopify") != -1 && newListUrl[i].LastIndexOf(".") != -1)
                {                  
                    newListUrl[i] = newListUrl[i].Insert(newListUrl[i].LastIndexOf("."), "_medium");
                    counterLargeImage++;
                    if(counterLargeImage > 0 && indexLarge < 0)
                    {
                        indexLarge = i;
                    }
                }
            }

            thumbnailParent.gameObject.SetActive(false);

            // Downloads the image together and process as they finish        
            int countImages = 0;

            try
            {
                while (countImages < newListUrl.Count)  
                {
                    var finished = await downloader.Download(newListUrl[countImages]);

                    AddThumbnail(finished);
                    countImages++;

                }
            }
            catch (Exception e)
            {
                ScutiLogger.LogException(e);
            }

            Destroy(downloader.gameObject);
            //DownloadLargeImagen(indexLarge);
        }
     
        async void DownloadLargeImage(int indexLarge)
        {
            var downloader = ImageDownloader.New(false);
            string url = String.Empty;
            List<string> newListUrl = new List<string>(m_Urls);

            if (newListUrl.Count > 0)
            {
                if (indexLarge >= 0)
                {
                    url = newListUrl[indexLarge];
                }
                else
                {
                    url = newListUrl[0];
                }
            }

            if (!String.IsNullOrEmpty(url) && url.IndexOf("shopify") != -1 && url.LastIndexOf(".") != -1)
            {
                // if source its shopify
                url = url.Insert(url.LastIndexOf("."), "_1024x1024");
            }
            try
            {
                var finished = await downloader.Download(url);
                if (finished != null)
                {                    
                   AddDisplayImage(finished); 
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
            // If this is the first texture, set it to display
            if (Data.TextureCount() == 1) {
                imageDisplay.sprite = Data.GetTexture(0).ToSprite();
                m_Index = 0;
            }
        }

        void AddThumbnail(Texture2D texture2D) {

            var instance = Instantiate(thumbnailPrefab, thumbnailParent);
            instance.hideFlags = HideFlags.DontSave;
            //instance.transform.SetSiblingIndex(instance.transform.childCount - 1);
            var index = m_Thumbs.Count; 
            m_Thumbs.Add(instance);

            if (m_Thumbs.Count > 1) 
            {
                // reset position of content thumbnails
                thumbnailParent.anchoredPosition = new Vector2(0, thumbnailParent.anchoredPosition.y);
                thumbnailParent.gameObject.SetActive(true);
            }              
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
                    LoadVariantImage(productVariant.Image);
                }
                else 
                {
                    if(Data.TextureCount() <= 0)
                        DownloadLargeImage(-1);
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
            imageDisplay.sprite = tex.ToSprite();
        }
    }
}
