# ECAN
Simple ECAN usage with .Net Framework



## Singleton API

```C#
ECANAPI._.Initialize();
ECANAPI._.OpenChannel();
ECANAPI._.Update(float dt);
ECANAPI._.OpenDevice(byte baud_rate);
ECANAPI._.ResetCAN();
ECANAPI._.WriteDataToChannel(string id, int nud_length, List<string> data, bool chb_extended, bool chb_remote);
ECANAPI._.ReadDataFromChannel();
ECANAPI._.GetBoardInfo();
ECANAPI._.CloseDecive();
```

