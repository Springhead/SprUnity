// https://qiita.com/k_yanase/items/fb64ccfe1c14567a907d
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SprUnity {

    /// <summary>
    /// テーブルの管理クラス
    /// </summary>
    [System.Serializable]
    public class TableBase<TKey, TValue, Type> where Type : KeyAndValue<TKey, TValue> {
        [SerializeField]
        private List<Type> list = new List<Type>();
        private Dictionary<TKey, TValue> table;


        public Dictionary<TKey, TValue> GetTable() {
            if (table == null) {
                table = ConvertListToDictionary(list);
            }
            return table;
        }

        /// <summary>
        /// Editor Only
        /// </summary>
        public List<Type> GetList() {
            return list;
        }

        static Dictionary<TKey, TValue> ConvertListToDictionary(List<Type> list) {
            Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();
            foreach (KeyAndValue<TKey, TValue> pair in list) {
                if (pair.Key == null) continue;
                dic.Add(pair.Key, pair.Value);
            }
            return dic;
        }

        static List<Type> ConvertDictionaryToList(Dictionary<TKey, TValue> dict) {
            List<Type> list = new List<Type>();
            foreach (var pair in dict) {
                list.Add(new KeyAndValue<TKey, TValue>(pair.Key, pair.Value) as Type);
            }
            return list;
        }

        /// <summary>
        /// APIs
        /// </summary>
        public void Add(Type keyAndValue) {
            if (keyAndValue.Key == null) return;
            list.Add(keyAndValue);
            table = null;
        }
        /*
        public void Set(TKey key, TValue value) {
            table = GetTable();
            table[key] = value;
            list = ConvertDictionaryToList(table);
        }
        */

        public void Clear() {
            list.Clear();
            table = null;
        }

        public void CleanNullKey() {
            for (int i = list.Count - 1; i >= 0; i--) {
                if (list[i].Key == null) {
                    list.Remove(list[i]);
                }
            }
        }
    }

    /// <summary>
    /// シリアル化できる、KeyValuePair
    /// </summary>
    [System.Serializable]
    public class KeyAndValue<TKey, TValue> {
        public TKey Key;
        public TValue Value;

        public KeyAndValue(TKey key, TValue value) {
            Key = key;
            Value = value;
        }
        public KeyAndValue(KeyValuePair<TKey, TValue> pair) {
            Key = pair.Key;
            Value = pair.Value;
        }
    }
}
