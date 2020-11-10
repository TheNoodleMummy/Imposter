using Imposter.services;
using Mummybot.Enums;
using Mummybot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

namespace Imposter
{
    public class DataService : BaseService
    {
        private const string Queensjson = "Queens.json", Kingsjson = "Kings.json", ImposterWhitelist = "whitelist.json";

        public LogService LogService { get; set; }

        public ObservableCollection<ulong> WhitelistedIds { get; set; } = new ObservableCollection<ulong>();

        public ObservableCollection<(ulong id, int count)> ImposterKings { get; set; } = new ObservableCollection<(ulong id, int count)>();

        public ObservableCollection<(ulong id, int count)> ImposterQueens { get; set; } = new ObservableCollection<(ulong id, int count)>();
        

        public DataService(LogService logService)
        {
            LogService = logService;
        }

        private void ImposterQueens_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LogService.LogInformation($"Queens changed {e.Action} replaced items {e.NewItems}", LogSource.DataService);
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                LogService.LogDebug($"saveing queens", LogSource.DataService);
                File.WriteAllText(Queensjson, JsonConvert.SerializeObject(ImposterQueens));
            }
        }

        private void Imposterkings_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LogService.LogInformation($"Kings changed {e.Action} replaced items {e.NewItems}", LogSource.DataService);
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                LogService.LogDebug($"saveing kings", LogSource.DataService);
                File.WriteAllText(Kingsjson, JsonConvert.SerializeObject(ImposterKings));
            }

        }

        private void WhitelistedIds_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LogService.LogDebug($"Imposter Whitelist changed {e.Action}", LogSource.DataService);
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                LogService.LogDebug($"saveing Imposter Whitelist", LogSource.DataService);
                File.WriteAllText(ImposterWhitelist, JsonConvert.SerializeObject(WhitelistedIds));
            }
        }

        public override Task InitialiseAsync(IServiceProvider services)
        {
            if (!File.Exists(Kingsjson))
                File.WriteAllText(Kingsjson, JsonConvert.SerializeObject(ImposterKings));

            if (!File.Exists(Queensjson))
                File.WriteAllText(Queensjson, JsonConvert.SerializeObject(ImposterQueens));

            if (!File.Exists(ImposterWhitelist))
                File.WriteAllText(ImposterWhitelist, JsonConvert.SerializeObject(WhitelistedIds));

            var txt = File.ReadAllText(ImposterWhitelist);
            WhitelistedIds = JsonConvert.DeserializeObject<ObservableCollection<ulong>>(txt);

            txt = File.ReadAllText(Kingsjson);
            ImposterKings = JsonConvert.DeserializeObject<ObservableCollection<(ulong id, int count)>>(txt);

            txt = File.ReadAllText(Queensjson);
            ImposterQueens = JsonConvert.DeserializeObject<ObservableCollection<(ulong id, int count)>>(txt);

            WhitelistedIds.CollectionChanged += WhitelistedIds_CollectionChanged;
            ImposterKings.CollectionChanged += Imposterkings_CollectionChanged;
            ImposterQueens.CollectionChanged += ImposterQueens_CollectionChanged;

            return base.InitialiseAsync(services);
        }
    }



}
