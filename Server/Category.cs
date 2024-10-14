using System.Text.Json;

public class Category
{

    private static List<Dictionary<string, object>> categories = new List<Dictionary<string, object>>
    {
        new Dictionary<string, object> { { "cid", 1 }, { "name", "Beverages" } },
        new Dictionary<string, object> { { "cid", 2 }, { "name", "Condiments" } },
        new Dictionary<string, object> { { "cid", 3 }, { "name", "Confections" } }
    };

    public List<Dictionary<string, object>> GetAllCategories()
    {
        return categories;
    }

    public Dictionary<string, object> GetCategory(int id)
    {
        foreach (var dict in categories)
        {
            if ((int)dict["cid"] == id)
            {
                return dict;
            }
        }
        return null;
    }
    public void AddCategory(Dictionary<string, object> category)
    {
        categories.Add(category);
    }

    public void UpdateCategory(int id, string updatedName)
    {
    var category = GetCategory(id);
    category["name"] = updatedName;
        
    }

    public void DeleteCategory(int id)
    {
        var category = GetCategory(id);

        categories.Remove(category);
    }
}





