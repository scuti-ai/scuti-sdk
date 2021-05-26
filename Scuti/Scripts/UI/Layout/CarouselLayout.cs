 
﻿using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scuti {
    // TODO:
    // Add API to access widgets and slots and to remove them
    public class CarouselLayout : MonoBehaviour {
        [SerializeField] Transform m_Container;
        [SerializeField] CarouselSlot m_SlotTemplate;

        List<CarouselSlot> m_Slots = new List<CarouselSlot>();
        Queue<GameObject> m_Queue = new Queue<GameObject>();
        bool m_IsBusy;

        CarouselSlot LatestSlot {
            get {
                if (m_Slots.Count == 0) return null;
                return m_Slots[m_Slots.Count - 1];
            }
        }

        public void Add(GameObject go) {
            // elements are added to a queue so that they can be processed
            // one by one. This class internally works with a 1 frame delay
            // so have have to store the Add requests in a queue
            m_Queue.Enqueue(go);
        }

        public void Clear() {
            foreach (var slot in m_Slots)
                Destroy(slot.gameObject);
            m_Queue.Clear();
            m_IsBusy = false;
        }

        void Update() {
            if (m_Queue.Count > 0 && !m_IsBusy)
                Process(m_Queue.Dequeue());
        }

        async void Process(GameObject go) {
            m_IsBusy = true;

            // If there are no slots (which also means no elements have been added to carousel)
            // we clone the template, enable it and add it to the list (so that LatestSlot != null)
            // and recursively call ourselves
            if (LatestSlot == null) {
                AddToNewSlot(go);
                return;
            }

            // Add the element to the slot. We have to check if after adding the element
            // the slot is full or not. But, the UI is taking a frame to update so we check next frame
            LatestSlot.Add(go);

            await TaskX.WaitForFrames(2);

            if (!LatestSlot.IsFull())
                m_IsBusy = false;
            else
                AddToNewSlot(go);

            //Runner.New().WaitForEndOfFrame(() => {
            //    // If the slot is NOT full after adding, all is OK. Make the instance un-busy
            //    // else if the slot IS full, we add to a new slot
            //});
        }

        void AddToNewSlot(GameObject go) {
            var newSlot = Instantiate(m_SlotTemplate, m_Container);
            newSlot.gameObject.hideFlags = HideFlags.DontSave;
            newSlot.gameObject.SetActive(true);
            m_Slots.Add(newSlot);
            Process(go);
        }
    }
} 
