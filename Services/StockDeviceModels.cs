// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.DeviceSimulation.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceSimulation.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceSimulation.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceSimulation.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceSimulation.Services
{
    public interface IStockDeviceModels
    {
        IEnumerable<DeviceModel> GetList();
        DeviceModel Get(string id);
    }

    public class StockDeviceModels : IStockDeviceModels
    {
        private const string EXT = ".json";

        private readonly IServicesConfig config;
        private readonly ILogger log;

        private List<string> deviceModelFiles;
        private List<DeviceModel> deviceModels;

        public StockDeviceModels(
            IServicesConfig config,
            ILogger logger)
        {
            this.config = config;
            this.log = logger;
            this.deviceModelFiles = null;
            this.deviceModels = null;
        }

        public IEnumerable<DeviceModel> GetList()
        {
            if (this.deviceModels != null) return this.deviceModels;

            const string STOCKMODEL = "StockModel";
            this.deviceModels = new List<DeviceModel>();

            try
            {
                var files = this.GetDeviceModelFiles();
                foreach (var f in files)
                {
                    var c = JsonConvert.DeserializeObject<DeviceModel>(File.ReadAllText(f));
                    c.Type = STOCKMODEL;
                    this.deviceModels.Add(c);
                }
            }
            catch (Exception e)
            {
                this.log.Error("Unable to load Device Model configuration",
                    () => new { e.Message, Exception = e });

                throw new InvalidConfigurationException("Unable to load Device Model configuration: " + e.Message, e);
            }

            return this.deviceModels;
        }

        public DeviceModel Get(string id)
        {
            var list = this.GetList();
            var item = list.FirstOrDefault(i => i.Id == id);
            if (item != null)
                return item;

            this.log.Warn("Device model not found", () => new { id });

            throw new ResourceNotFoundException("Device model not found with id: '" + id + "'.");
        }

        private List<string> GetDeviceModelFiles()
        {
            if (this.deviceModelFiles != null) return this.deviceModelFiles;

            this.log.Debug("Device models folder", () => new { this.config.DeviceModelsFolder });

            var fileEntries = Directory.GetFiles(this.config.DeviceModelsFolder);

            this.deviceModelFiles = fileEntries.Where(fileName => fileName.EndsWith(EXT)).ToList();

            this.log.Debug("Device model files", () => new { this.deviceModelFiles });

            return this.deviceModelFiles;
        }
    }
}
