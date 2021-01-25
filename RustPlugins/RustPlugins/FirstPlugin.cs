using ConVar;
using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("FirstPlugin", "Daan VM", "0.0.1")]
    public class FirstPlugin : RustPlugin
    {
        #region Config
        private ConfigData configData;

        class ConfigData
        {
            /*[JsonProperty(PropertyName = "Boolean")]
            public bool rep = true;*/
            [JsonProperty(PropertyName = "Location where you want to spawn (x)")]
            public int xValue = 0;
            [JsonProperty(PropertyName = "Location where you want to spawn (y)")]
            public int yValue = 0;
            [JsonProperty(PropertyName = "Location where you want to spawn (z)")]
            public int zValue = 0;
        }

        private bool LoadConfigVariables()
        {
            try
            {
                configData = Config.ReadObject<ConfigData>();
            }
            catch
            {
                return false;
            }
            SaveConfig(configData);
            return true;
        }


        protected override void LoadDefaultConfig()
        {
            Puts("Creating new config file!");
            configData = new ConfigData();
            SaveConfig(configData);
        }

        void SaveConfig(ConfigData config)
        {
            Config.WriteObject(config, true);
        }
        #endregion

        #region Data
        private StoredData storedData;

        class StoredData
        {
            public List<string> chatlog = new List<string>();
            public Dictionary<string, Vector3> playerLocation = new Dictionary<string, Vector3>();            
        }

        void Loaded()
        {
            storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>("FirstPluginData");
            Interface.Oxide.DataFileSystem.WriteObject("FirstPluginData", storedData);
        }

        void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("FirstPluginData", storedData);
        }

        #endregion

        #region Init
        void Init()
        {
            permission.RegisterPermission("FirstPlugin.admin", this);
            if (!LoadConfigVariables())
            {
                Puts("Config file issue detected, please check syntax or delete the file");
                return;
            }
        }
        #endregion

        #region Hooks

        void OnPlayerChat(BasePlayer player, string message, Chat.ChatChannel channel)
        {
            Puts("OnPlayerChat works!");
            string output = string.Format("{0} - {1}({2}) - {3}", DateTime.Now.ToString("M/dd HH:mm:ss"), player.displayName, player.userID.ToString(), message);
            storedData.chatlog.Add(output);
            SaveData();
        }
        #endregion

        #region Methods

        void preTeleport(BasePlayer player)
        {
            player.StartSleeping();
            player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
            player.ClientRPCPlayer(null, player, "StartLoading");
            player.SendEntityUpdate();
        }

/*        bool checkTeleport(BasePlayer player)
        {
            //if(!player.IsBuildingBlocked() && player.IsWounded(), player.rad)
            string o = Interface.Oxide.CallHook("CanTeleport", player) as string;
        }*/

        #endregion

        #region Commands
        /*[ConsoleCommand("test")]
        private void testConsole()
        {
            configData.rep = !configData.rep;
            SaveConfig(configData);
        }

        [ChatCommand("test")]
        private void testChat(BasePlayer player)
        {
            if(!permission.UserHasPermission(player.userID.ToString(), "FirstPlugin.admin"))
            {
                SendReply(player, "You do not have the right permissions to use this command");
                return;
            }
            else
            {
                configData.rep = !configData.rep;
                SaveConfig(configData);
                SendReply(player, $"The config data was succesfully changed from {!configData.rep} to {configData.rep}");
            }
        }*/

        /*        [ChatCommand("teleport")]
                private void teleport(BasePlayer player)
                {
                    Vector3 prevLocation = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
                    Vector3 resourceHouseLocation = new Vector3(configData.xValue, configData.yValue, configData.zValue);
                    player.StartSleeping();
                    player.Teleport(resourceHouseLocation);
                    timer.Once(10f, () =>
                    {
                        player.StartSleeping();
                        player.Teleport(prevLocation);
                    });
                }*/

        [ChatCommand("teleport")]
        private void teleport(BasePlayer player)
        {
            Vector3 resourceHouseLocation = new Vector3(configData.xValue, configData.yValue, configData.zValue);
            
            if (storedData.playerLocation.ContainsKey(player.userID.ToString()))
            {
                storedData.playerLocation.Remove(player.userID.ToString());
                SaveData();
            }
            storedData.playerLocation.Add(player.userID.ToString(), new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z));
            SaveData();
            //if(checkTeleport)
            preTeleport(player);
            player.Teleport(resourceHouseLocation);
            timer.Once(10f, () =>
            {
                player.StartSleeping();
                player.Teleport(storedData.playerLocation[player.userID.ToString()]);
            });

        }

        [ChatCommand("test")]
        private void test(BasePlayer player)
        {

            SendReply(player, "lol ggg");
/*            string o = Interface.Oxide.CallHook("CanTeleport", player) as string;
            SendReply(player, "lol gg");
            Puts(o);*/
            Puts(Interface.Oxide.CallHook("CanTeleport", player).ToString());
        }

        #endregion
    }
}
