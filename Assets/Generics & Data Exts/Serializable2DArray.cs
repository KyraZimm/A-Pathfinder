using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class Serializable2DArray<T> : ISerializationCallbackReceiver {

    //unserialized data for editor
    private T[,] unserialized;
    public Serializable2DArray(T[,] array) { unserialized = array; }


    //pseudo funcs to act like array
    public int GetLength(int dim) { return unserialized.GetLength(dim); }
    public int Length { get { return unserialized.Length; } }
    public T[,] Array { get { return unserialized; } }
    


    //serialized data
    [SerializeField, HideInInspector] private SerializedItem<T>[] serialized;
    [SerializeField, HideInInspector] private int length0;
    [SerializeField, HideInInspector] private int length1;
    [System.Serializable] public struct SerializedItem<TItem> where TItem : T  {
        public int index0;
        public int index1;
        public TItem item;

        public SerializedItem(int index0, int index1, TItem item){
            this.index0 = index0;
            this.index1 = index1;
            this.item = item;
        }
    }


    //handle serialization & deserialization
    public void OnBeforeSerialize(){

        //if there is no array in this class, serialize an empty array
        if (unserialized == null) {
            serialized = new SerializedItem<T>[0];
            return;
        }

        serialized = new SerializedItem<T>[unserialized.Length];
        length0 = unserialized.GetLength(0);
        length1 = unserialized.GetLength(1);

        int i = 0;
        for (int x = 0; x < length0; x++) {
            for (int y = 0; y < length1; y++) {
                serialized[i] = new SerializedItem<T>(x, y, unserialized[x,y]);
                i++;
            }
        }
    }

    public void OnAfterDeserialize() {
        unserialized = new T[length0,length1];

        for (int i = 0; i < serialized.Length; i++)
            unserialized[serialized[i].index0, serialized[i].index1] = serialized[i].item;
    }

    /*public void ChangeArraySize(int newLength0, int newLength1) {
        //make sure array size is never negative
        if (newLength0 < 0) newLength0 = 0;
        if (newLength1 < 0) newLength1 = 0;

        //make new sized array, copy over elements that were already input
        T[,] copyTo = new T[newLength0, newLength1];
        for (int x = 0; x < newLength0; x++)
            for (int y = 0; y < newLength1; y++)
                copyTo[x, y] = (x < length0 && y < length1) ? unserialized[x, y] : default(T);

        //set new array
        unserialized = copyTo;
    }*/
    
}
