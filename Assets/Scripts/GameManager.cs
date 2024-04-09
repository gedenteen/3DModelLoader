using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _contentForButtons;
    [SerializeField] private GameObject _prefabButtonCreateItem;
    [SerializeField] private ModelsManager _modelsManager;
    [SerializeField] private UiController _uiController;

    private readonly string _urlGetList = "https://variant-unity-test-server.vercel.app/api/list";
    private readonly string _urlGetObject = "https://variant-unity-test-server.vercel.app/api/getObject";
    private readonly string _urlGetMaterial = "https://variant-unity-test-server.vercel.app/api/getMaterial";
    private readonly string _urlGetSprite = "https://variant-unity-test-server.vercel.app/static/";

    private ListOfItems _listOfItems;

    private Dictionary<string, Texture2D> _savedTextures = new Dictionary<string, Texture2D>();

    private async void Start()
    {
        await GetListOfItems();
        CreateButtonsForItems();
    }

    private async Task GetListOfItems()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(_urlGetList))
        {
            request.SendWebRequest();
            while (!request.isDone)
            {
                await Task.Yield();
            }

            // Проверка на наличие ошибок
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("GET запрос успешно выполнен: " + request.downloadHandler.text);
                _listOfItems = JsonConvert.DeserializeObject<ListOfItems>(request.downloadHandler.text);
            }
            else
            {
                Debug.Log("Произошла ошибка при выполнении GET запроса: " + request.error);
            }
        }
    }

    private async void CreateButtonsForItems()
    {
        foreach (Item item in _listOfItems.items)
        {
            GameObject goButtonCreateItem = Instantiate(_prefabButtonCreateItem, _contentForButtons.transform);
            ButtonCreateItem buttonCreateItem = goButtonCreateItem.GetComponent<ButtonCreateItem>();
            buttonCreateItem.textMeshName.text = String.Concat("Name: ", item.name);
            buttonCreateItem.textMeshId.text = String.Concat("Id: ", item.id.ToString());
            buttonCreateItem.imageIcon.sprite = await GetSprite(item.icon);
            buttonCreateItem.button.onClick.AddListener(() => CreateModelWithTextures(item.id));
            buttonCreateItem.button.onClick.AddListener(() => _uiController.SetStateModelView());
        }
    }

    private async Task<Sprite> GetSprite(string spriteName)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(_urlGetSprite + spriteName))
        {
            request.SendWebRequest();
            while (!request.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Получение текстуры из ответа
                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                // Преобразование текстуры в спрайт
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                // Установка спрайта в компонент Image
                return sprite;
            }
            else
            {
                Debug.LogError("Failed to download image: " + request.error);
                return null;
            }
        }
    }

    private void CreateModelWithTextures(int itemId)
    {
        if (!_modelsManager.CheckModelAlreadyCreated(itemId))
        {
            _modelsManager.DeactivateAllModels();
            GetListOfObject(itemId);
        }
        else
        {
            _modelsManager.ActivateModelById(itemId);
        }
    }

    private async Task GetListOfObject(int id)
    {
        // Создание объекта для передачи данных
        PostDataInt data = new PostDataInt(id);

        // Преобразование объекта в JSON
        string jsonData = JsonConvert.SerializeObject(data);

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(_urlGetObject, jsonData))
        {
            // Установка заголовка Content-Type для указания типа данных JSON
            request.SetRequestHeader("Content-Type", "application/json");

            // Отправка данных в теле запроса
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));

            request.SendWebRequest();
            while (!request.isDone)
            {
                await Task.Yield();
            }

            // Проверка на наличие ошибок
            if (request.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log("POST запрос успешно выполнен: " + request.downloadHandler.text);
                ListOfObjects listOfObjects = JsonConvert.DeserializeObject<ListOfObjects>(request.downloadHandler.text);
                // Debug.Log($"objectData listOfItemsFullData.objects.Count={listOfObjects.objects.Count}");
                // foreach (ObjectData objectData in listOfObjects.objects)
                // {
                //     Debug.Log($"objectData position={objectData.transform.position} material={objectData.material}");
                // }

                CreateModelAndTextures(id, listOfObjects);
            }
            else
            {
                Debug.Log("Произошла ошибка при выполнении POST запроса: " + request.error);
            }
        }

    }

    private void CreateModelAndTextures(int id, ListOfObjects listOfObjects)
    {
        // Проверка наличия данных
        if (listOfObjects != null && listOfObjects.objects != null)
        {
            // Создание родительского объекта для всех создаваемых объектов
            GameObject parentObject = new GameObject("ModelParent");

            // Создание объектов для каждого элемента в списке объектов
            foreach (ObjectData objectData in listOfObjects.objects)
            {
                // Создание пустого игрового объекта
                GameObject gameObject = new GameObject("Object");

                // Присоединение созданного объекта к родительскому объекту
                gameObject.transform.SetParent(parentObject.transform);

                // Применение данных о трансформации
                gameObject.transform.position = objectData.transform.position;
                gameObject.transform.rotation = objectData.transform.rotation;

                // Добавление компонента MeshFilter
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

                // Создание сетки из данных
                Mesh mesh = new Mesh();
                mesh.vertices = objectData.mesh.positions.ToArray();
                mesh.normals = objectData.mesh.normals.ToArray();
                mesh.uv = objectData.mesh.uvs.ToArray();
                mesh.triangles = objectData.mesh.indices.ToArray();

                // Применение сетки к компоненту MeshFilter
                meshFilter.mesh = mesh;

                // Добавление компонента MeshRenderer
                MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

                // Создание и применение материала
                Material material = new Material(Shader.Find("Standard"));
                GetMaterialData(material, objectData.material);
                meshRenderer.material = material;
            }

            _modelsManager.AddModel(id, parentObject);
        }
        else
        {
            Debug.LogError("Data for model creation is missing!");
        }
    }

    private async Task GetMaterialData(Material material, string guid)
    {
        // Создание объекта для передачи данных
        PostDataString data = new PostDataString(guid);

        // Преобразование объекта в JSON
        string jsonData = JsonConvert.SerializeObject(data);

        // Отправка POST-запроса
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(_urlGetMaterial, jsonData))
        {
            // Установка заголовка Content-Type для указания типа данных JSON
            www.SetRequestHeader("Content-Type", "application/json");

            // Отправка данных в теле запроса
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));

            // Получение ответа
            www.SendWebRequest();
            while (!www.isDone)
            {
                await Task.Yield();
            }

            // Проверка на наличие ошибок
            if (www.result == UnityWebRequest.Result.Success)
            {
                // Debug.Log("POST запрос успешно выполнен: " + www.downloadHandler.text);
                MaterialData materialData = JsonConvert.DeserializeObject<MaterialData>(www.downloadHandler.text);
                // Debug.Log($"materialData.basecolor={materialData.basecolor}");

                if (!String.IsNullOrEmpty(materialData.basecolor))
                {
                    await SetTexture(material, materialData.basecolor);
                }
                if (!String.IsNullOrEmpty(materialData.normal))
                {
                    await SetTexture(material, materialData.normal, "_BumpMap"); // карта нормалей
                }
                if (!String.IsNullOrEmpty(materialData.roughness))
                {
                    await SetTexture(material, materialData.roughness, "_MetallicGlossMap"); // карта шероховатостей
                }
            }
            else
            {
                Debug.Log("Произошла ошибка при выполнении POST запроса: " + www.error);
            }
        }
    }

    private async Task SetTexture(Material material, string basecolorName)
    {
        // Отправка запроса на загрузку изображения
        string url = String.Concat(_urlGetSprite, basecolorName);
        Debug.Log($"SetTexture: url={url}");

        if (_savedTextures.ContainsKey(basecolorName))
        {
            Debug.Log($"SetTexture: i already have Texture with name={basecolorName}");
            material.mainTexture = _savedTextures[basecolorName];
            return;
        }

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(_urlGetSprite + basecolorName))
        {
            www.SendWebRequest();
            while (!www.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Получение текстуры из ответа
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                // Применение текстуры к Renderer
                if (material != null)
                {
                    material.mainTexture = texture;
                    _savedTextures.Add(basecolorName, texture);
                    Debug.Log($"SetTexture: saving Texture with name={basecolorName}");
                }
                else
                {
                    Debug.LogWarning("Target Renderer is not assigned!");
                }
            }
            else
            {
                Debug.LogError("Failed to download image: " + www.error);
            }
        }
    }

    private async Task SetTexture(Material material, string textureName, string propertyName)
    {
        // Отправка запроса на загрузку изображения
        string url = String.Concat(_urlGetSprite, textureName);
        Debug.Log($"SetTexture: url={url}");

        if (_savedTextures.ContainsKey(textureName))
        {
            Debug.Log($"SetTexture: i already have Texture with name={textureName}");
            material.SetTexture(propertyName, _savedTextures[textureName]);
            return;
        }

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            www.SendWebRequest();
            while (!www.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Получение текстуры из ответа
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                // Применение текстуры к материалу
                if (material != null)
                {
                    material.SetTexture(propertyName, texture);
                    _savedTextures.Add(textureName, texture);
                    Debug.Log($"SetTexture: saving Texture with name={textureName}");
                }
                else
                {
                    Debug.LogWarning("Material is not assigned!");
                }
            }
            else
            {
                Debug.LogError("Failed to download image: " + www.error + " url=" + url);
            }
        }
    }
}
