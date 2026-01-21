using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Managers.Adapter;

namespace Fish.Clean
{
    public class API : MonoBehaviour
    {
        async void Start()
        {
            var headers = new Dictionary<string, string> { { "Accept", "application/json" } };
            using var adapter = new HttpAdapter("https://jsonplaceholder.typicode.com", headers);
            try
            {
                var result = await adapter.GetAsync("todos/1");
                Debug.Log("API result: " + (result == null ? "null" : JsonConvert.SerializeObject(result)));
            }
            catch (Exception ex)
            {
                Debug.LogError($"API error: {ex}");
            }
        }
    
        void Update() { }
    }
}