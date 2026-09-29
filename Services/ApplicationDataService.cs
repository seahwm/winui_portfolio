using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Xml.Linq;
using winui_portfolio.Models;

namespace winui_portfolio.Services
{
    public static class ApplicationDataService
    {
        public static ObservableCollection<Snapshot> Snapshots { get; set; } =
            new ObservableCollection<Snapshot>();

        public static ObservableCollection<Models.AssetType> AssetTypes { get; set; } =
           new ObservableCollection<Models.AssetType>();

        public static ObservableCollection<string> StockTypes { get; set; } =
            new ObservableCollection<string>();

        public static Dictionary<string, AssetType> AssetTypeMap { get; set; } = new Dictionary<string, AssetType>();

        private static string DATA_FOLDER_PATH = "./data";

        private static string DATA_FILE_PATH = "E:\\___workspace\\______________note\\financial json" + "\\data.json";
        static ApplicationDataService()
        {
            LoadJsonData();
        }

        private static void LoadJsonData()
        {
            if (!Directory.Exists(DATA_FOLDER_PATH))
            {
                Directory.CreateDirectory(DATA_FOLDER_PATH);
            }

            if (!File.Exists(DATA_FILE_PATH))
            {
                File.Create(DATA_FILE_PATH).Close();
            }

            string jsonStr = File.ReadAllText(DATA_FILE_PATH, Encoding.UTF8);
            if (jsonStr == null || jsonStr.Length == 0)
            {
                return;
            }
            JsonNode? rootNode = JsonNode.Parse(jsonStr);
            if (rootNode == null)
            {
                return;
            }
            ParseJsonNode(rootNode);
            var sorted = Snapshots.OrderByDescending(s => s.Date).ToList();
            Snapshots.Clear();
            foreach (var item in sorted)
            {
                Snapshots.Add(item);
            }
        }

        private static void LoadAssetType(JsonArray assetTypeArr)
        {
            foreach (var assetTypeItm in assetTypeArr)
            {
                AssetType assetType = new AssetType();
                assetType.Name = assetTypeItm?["name"]?.ToString() ?? string.Empty;
                if (string.Empty.Equals(assetType.Name))
                {
                    continue;
                }
                assetType.IsValOnly = assetTypeItm?["isValOnly"]?.GetValue<bool>() ?? false;
                assetType.IsRetirement = assetTypeItm?["isRetirement"]?.GetValue<bool>() ?? false;
                assetType.IsParent = assetTypeItm?["isParent"]?.GetValue<bool>() ?? false;
                assetType.IsDebt = assetTypeItm?["isDebt"]?.GetValue<bool>() ?? false;
                assetType.SysRec = assetTypeItm?["sysRec"]?.GetValue<bool>() ?? false;
                AssetTypes.Add(assetType);
                AssetTypeMap[assetType.Name] = assetType;
            }
        }

        private static void LoadStockType(JsonArray stockTypeArr)
        {
            StockTypes.Clear();
            foreach (var stockTypeItm in stockTypeArr)
            {
                string name = stockTypeItm?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(name) && !StockTypes.Contains(name))
                {
                    StockTypes.Add(name);
                }
            }
        }

        private static void ParseJsonNode(JsonNode node)
        {
            LoadAssetType(node["assetType"] as JsonArray ?? []);
            LoadStockType(node["stockType"] as JsonArray ?? []);

            if (node["data"] is JsonArray dataArray)
            {
                foreach (var dateItem in dataArray)
                {
                    Snapshot snapshot = new Snapshot();
                    string date = dateItem?["date"]?.ToString() ?? "Unknown";
                    snapshot.Date = DateOnly.Parse(date);
                    snapshot.UsdRate = dateItem?["usdRate"]?.GetValue<decimal>() ?? Decimal.Zero;
                    ObservableCollection<Asset> assets = [];
                    if (dateItem?["assets"] is JsonArray assetsArray)
                    {
                        foreach (var assetMap in assetsArray)
                        {
                            Asset asset = new Asset();
                            // 获取资产基本信息（有的叫 name，有的叫 assetName）
                            string name = assetMap?["name"]?.ToString() ?? assetMap?["assetName"]?.ToString() ?? "未命名";
                            decimal value = assetMap?["value"]?.GetValue<decimal>() ?? 0;
                            string currency = assetMap?["currency"]?.ToString() ?? "";
                            decimal cost = assetMap?["cost"]?.GetValue<decimal>() ?? 0;
                            string type = assetMap?["type"]?.ToString() ?? "";
                            asset.Name = name;
                            asset.Value = value;
                            asset.Cost = cost;
                            asset.Currency = currency;
                            asset.Remark = assetMap?["remark"]?.ToString() ?? string.Empty;
                            asset.AssetType = AssetTypeMap.GetValueOrDefault(type);
                            if (asset.AssetType == null && type.Length != 0)
                            {
                                throw new Exception($"  - 资产类型未找到: {type}");

                            }
                            // 如果包含嵌套的股票/持仓列表 (content)
                            if (assetMap?["content"] is JsonArray contentArray)
                            {
                                foreach (var stockNode in contentArray)
                                {
                                    Stock stock = new Stock();
                                    stock.Id = stockNode?["id"]?.ToString() ?? Guid.NewGuid().ToString();
                                    stock.Name = stockNode?["name"]?.ToString() ?? "";
                                    stock.Symbol = stockNode?["symbol"]?.ToString() ?? "";
                                    stock.StockType = stockNode?["stockType"]?.ToString() ?? stockNode?["sector"]?.ToString() ?? "";
                                    stock.Units = stockNode?["units"]?.GetValue<decimal>() ?? Decimal.Zero;
                                    stock.AvgPrice = stockNode?["avgPrice"]?.GetValue<decimal>() ?? Decimal.Zero;
                                    stock.CurrentPrice = stockNode?["currentPrice"]?.GetValue<decimal>() ?? Decimal.Zero;
                                    stock.DividendYield = stockNode?["dividendYield"] != null && decimal.TryParse(stockNode["dividendYield"]?.ToString(), out var dy) ? dy : Decimal.Zero;
                                    var accProfitNode = stockNode?["accumulatedProfit"] ?? stockNode?["accumulatedDividend"];
                                    stock.AccumulatedProfit = accProfitNode != null && decimal.TryParse(accProfitNode.ToString(), out var ad) ? ad : Decimal.Zero;
                                    stock.Currency = stockNode?["currency"]?.ToString() ?? asset.Currency ?? "MYR";
                                    stock.Remark = stockNode?["remark"]?.ToString() ?? string.Empty;
                                    asset.Stocks.Add(stock);
                                }
                            }
                            assets.Add(asset);
                            Debug.WriteLine($"  - 资产: {name} | 价值: {value} {currency} | 股票数: {asset.Stocks.Count}");
                        }
                    }
                    snapshot.Assets = assets;
                    Snapshots.Add(snapshot);
                }

            }

        }

        public static void SaveSnapshots()
        {
            JsonNode? rootNode = null;
            if (File.Exists(DATA_FILE_PATH))
            {
                string jsonStr = File.ReadAllText(DATA_FILE_PATH, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(jsonStr))
                {
                    rootNode = JsonNode.Parse(jsonStr);
                }
            }
            rootNode ??= new JsonObject();
            // 2. Update data array in rootNode
            JsonArray dataArr = new JsonArray();
            foreach (var snapshot in Snapshots)
            {
                JsonObject snapshotObj = new JsonObject
                {
                    ["date"] = snapshot.Date.ToString("yyyy-MM-dd"),
                    ["usdRate"] = snapshot.UsdRate
                };
                JsonArray assetsArr = new JsonArray();
                foreach (var asset in snapshot.Assets)
                {
                    JsonObject assetObj = new JsonObject
                    {
                        ["name"] = asset.Name,
                        ["value"] = asset.Value,
                        ["currency"] = asset.Currency,
                        ["cost"] = asset.Cost,
                        ["type"] = asset.AssetType?.Name ?? "",
                        ["remark"] = asset.Remark
                    };

                    if (asset.Stocks != null && asset.Stocks.Count > 0)
                    {
                        JsonArray contentArr = new JsonArray();
                        foreach (var stock in asset.Stocks)
                        {
                            JsonObject stockObj = new JsonObject
                            {
                                ["name"] = stock.Name,
                                ["symbol"] = stock.Symbol,
                                ["stockType"] = stock.StockType,
                                ["units"] = stock.Units,
                                ["avgPrice"] = stock.AvgPrice,
                                ["currentPrice"] = stock.CurrentPrice,
                                ["dividendYield"] = stock.DividendYield,
                                ["accumulatedProfit"] = stock.AccumulatedProfit,
                                ["currency"] = stock.Currency ?? "MYR",
                                ["remark"] = stock.Remark
                            };
                            contentArr.Add(stockObj);
                        }
                        assetObj["content"] = contentArr;
                    }

                    assetsArr.Add(assetObj);
                }
                snapshotObj["assets"] = assetsArr;
                dataArr.Add(snapshotObj);
            }
            rootNode["data"] = dataArr;
            var options = new JsonSerializerOptions
            {
                // 关键属性：指定编码器不转义字符（如中文字符）
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                // 可选：让生成的 JSON 自动缩进换行，排版更漂亮
                WriteIndented = true
            };
            string updatedJson = rootNode.ToJsonString(options);
            File.WriteAllText(DATA_FILE_PATH, updatedJson, Encoding.UTF8);
        }

        public static void SaveAssetType()
        {

            JsonNode? rootNode = null;
            if (File.Exists(DATA_FILE_PATH))
            {
                string jsonStr = File.ReadAllText(DATA_FILE_PATH, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(jsonStr))
                {
                    rootNode = JsonNode.Parse(jsonStr);
                }
            }
            rootNode ??= new JsonObject();

            // 3. Update assetType array in rootNode
            JsonArray assetTypeArr = new JsonArray();
            foreach (var item in AssetTypes)
            {
                JsonObject itemObj = new JsonObject
                {
                    ["name"] = item.Name,
                    ["isValOnly"] = item.IsValOnly,
                    ["isRetirement"] = item.IsRetirement,
                    ["isParent"] = item.IsParent,
                    ["isDebt"] = item.IsDebt
                };
                if (item.SysRec)
                {
                    itemObj["sysRec"] = true;
                }
                assetTypeArr.Add(itemObj);
            }
            rootNode["assetType"] = assetTypeArr;
            var options = new JsonSerializerOptions
            {
                // 关键属性：指定编码器不转义字符（如中文字符）
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                // 可选：让生成的 JSON 自动缩进换行，排版更漂亮
                WriteIndented = true
            };
            string updatedJson = rootNode.ToJsonString(options);
            File.WriteAllText(DATA_FILE_PATH, updatedJson, Encoding.UTF8);
        }

        public static void SaveStockType()
        {
            JsonNode? rootNode = null;
            if (File.Exists(DATA_FILE_PATH))
            {
                string jsonStr = File.ReadAllText(DATA_FILE_PATH, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(jsonStr))
                {
                    rootNode = JsonNode.Parse(jsonStr);
                }
            }
            rootNode ??= new JsonObject();

            JsonArray stockTypeArr = new JsonArray();
            foreach (var item in StockTypes)
            {
                stockTypeArr.Add(JsonValue.Create(item));
            }
            rootNode["stockType"] = stockTypeArr;
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            string updatedJson = rootNode.ToJsonString(options);
            File.WriteAllText(DATA_FILE_PATH, updatedJson, Encoding.UTF8);
        }

        public static void SaveAll()
        {
            SaveAssetType();
            SaveStockType();
            SaveSnapshots();
        }
    }

}
