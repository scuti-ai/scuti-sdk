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

            if(Data.TextureCount()>m_Index)
                imageDisplay.sprite = Data.GetTexture(m_Index).ToSprite();
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

        async void DownloadImages() {
            var downloader = ImageDownloader.New(false);
            var downloads = new List<Task<Texture2D>>();

             

            Data.URLs.ForEach(x => downloads.Add(downloader.Download(x)));
            thumbnailParent.gameObject.SetActive(false);
            // Downloads the iamges together and process as they finish         

            try
            {
                while (downloads.Count > 0)
                {
                    var finished = await Task.WhenAny(downloads);
                    downloads.Remove(finished);
                    if (finished.Result != null)
                    {
                        AddDisplayImage(finished.Result);
                        AddThumbnail(finished.Result);

                        // Hack, add scrollable area instead
                        //if (Data.TextureCount() > 3) break;
                    }
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

            var index = m_Thumbs.Count;
            m_Thumbs.Add(instance);

            if(m_Thumbs.Count>1)
                thumbnailParent.gameObject.SetActive(true);
            var image = instance.GetComponent<Image>();
            image.sprite = texture2D.ToSprite();
            var button = instance.GetComponent<Button>();
            button.onClick.AddListener(() => {
                m_Index = index;
                RefreshDisplay();
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
            }
        }

        async void LoadVariantImage(string image)
        {
            var downloader = ImageDownloader.New(true);
            var tex = await downloader.Download(image);
            Data.ClearVariantTexture();
            Data.AddVariantTexture(tex);
            imageDisplay.sprite = tex.ToSprite();
        }
    }
}
