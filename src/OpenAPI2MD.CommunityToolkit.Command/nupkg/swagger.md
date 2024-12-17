 # 北向接口(3.0.0) 
<!-- @import "[TOC]" {cmd="toc" depthFrom=2 depthTo=3 orderedList=false} -->

<!-- code_chunk_output -->

- [一、系统概述](#一-系统概述)
- [二、系统总览](#二-系统总览)
- [三、一般约定](#三-一般约定)
- [四、功能说明](#四-功能说明)
- [1、认证与授权](#1-认证与授权)
  - [用户登录授权](#用户登录授权)
  - [检查用户权限](#检查用户权限)
- [2、查询功能接口](#2-查询功能接口)
  - [获取活动告警列表](#获取活动告警列表)
  - [获取缓存信息](#获取缓存信息)
  - [执行CiCode](#执行cicode)
  - [获取报警历史数据](#获取报警历史数据)
  - [获取系统层级](#获取系统层级)
  - [检查集群状态](#检查集群状态)
  - [获取PO系统时间](#获取po系统时间)
  - [获取点位基本信息](#获取点位基本信息)
  - [获取告警基本信息](#获取告警基本信息)
  - [获取点位实时数据](#获取点位实时数据)
  - [获取波形实时数据](#获取波形实时数据)
  - [获取点位历史数据](#获取点位历史数据)
  - [获取点位指标结果](#获取点位指标结果)
- [3、故障录波接口](#3-故障录波接口)
  - [获取系统发生告警](#获取系统发生告警)
  - [获取告警关联故障录波](#获取告警关联故障录波)
  - [获取障录波详情](#获取障录波详情)
- [4、订阅功能接口](#4-订阅功能接口)
  - [点位数据变化订阅](#点位数据变化订阅)
  - [点位数据变化推送](#点位数据变化推送)
  - [点位时间间隔订阅](#点位时间间隔订阅)
  - [点位时间间隔推送](#点位时间间隔推送)
  - [告警触发订阅](#告警触发订阅)
  - [告警触发推送](#告警触发推送)
- [5、点位控制功能接口](#5-点位控制功能接口)
  - [控制点位数值](#控制点位数值)
- [6、配置功能接口](#6-配置功能接口)
  - [](#)

<!-- /code_chunk_output -->



## 一、系统概述

&nbsp;&nbsp;&nbsp;&nbsp;本文描述了一种从施耐德电气Power Operation数据采集与监视控制系统（下称“PO”）获取数据的数据交换接口（下称“北向接口”）的设计与使用方法。<br>
&nbsp;&nbsp;&nbsp;&nbsp;本文的目标读者为具有编程经验的PO上位应用（下称“上位应用”）开发者。

## 二、系统总览

#### &nbsp;&nbsp;&nbsp;&nbsp;2.12.1系统架构<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;系统架构如上图所示，每个北向接口的部署实例应当独占一个PO客户端，一个上位应用可以与一个或多个北向接口实例对接。<br>

#### &nbsp;&nbsp;&nbsp;&nbsp;2.2上位应用的架构考虑<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PO客户端的可编程接口的一部分方法存在诸如只允许单线程访问等限制，北向接口并不能突破这些限制，北向接口也受制于PO客户端的可编程接口的其他诸如性能和并发等特性的限制。<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;北向接口中约定查询类型的请求优先级高于订阅类型的请求，北向接口的设计与实现并不提供相同请求类型接口调用的优先级概念。由于查询类型的请求会中断订阅类型请求的内部轮询机制，因此大数据量的查询请求会严重影响现存订阅的实时性。建议上层应用在设计是尽可能避免查询和订阅共同使用，或将查询和订阅分布到由不同PO客户端支持的北向接口实例中。<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;例如：当北向接口收到点位值订阅发起请求后，受限于PO可编程接口的并发限制，在北向接口内部会开启一个无限循环对所有被订阅对象的最后更新值进行轮询；在此订阅内部的轮询循环存续期间，如果有点位值查询请求到达，由于PO可编程接口不允许单个客户端上对点位实时值进行多线程并发操作，为了满足点位值查询请求，订阅轮询循环不得不挂起，让查询请求先行，这一挂起特性会导致订阅的采样频率发生变化而无法被估计。<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;对于相同的请求类型，北向接口以相同的优先级结合先到先得的原则以最大努力进行满足。北向接口并不能感知相同请求类型中更紧迫的请求，也无法优先为这些请求提供服务。<br>
在上位应用设计时，应当考虑到对PO客户端资源的争用情况，根据业务需要，规划所需的PO客户端/北向接口组合的数量，并将不同的业务分配到不同的PO客户端/北向接口组合中，以增加关键资源数量并规划业务-关键资源的分配与对应情况。<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;通过北向接口访问PO数据受制于PO客户端以及北向接口本身的可用性，在有高可用需求的业务场景下，上位应用应当规划一个以上的PO客户端/北向接口组合，并在上位应用内部实现对北向接口调用的故障转移。<br>

## 三、一般约定

#### &nbsp;&nbsp;&nbsp;&nbsp;3.1通信和编码约定<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;查询类接口遵循HTTPS通信协议，设计采用REST风格。<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;订阅类接口依照先订阅后推送原则，除了遵循HTTPS通信协议接受订阅发起外，还遵循WebSocket通信协议推送订阅的数据。一个订阅的生命周期从订阅发起时刻（HTTP POST完成）开始，到其对应的WebSocket连接主动或被动断开，或WebSocket连接超时结束，分为发起和推送两个阶段。通过调用订阅类接口只能保证在推送阶段（从接收WebSocket握手完成开始，到WebSocket主动或被动断开结束）的数据完整性。订阅不能溯及过往，也无法通过订阅接口获得两次订阅之间的数据。为了维护WebSocket的连接状态，北向接口会每30秒在WebSocket管道内发送Ping控制包以维护心跳，上位应用在收到心跳包后应立即返回Pong控制包。如果北向接口在60秒内未能收到上位应用的心跳信息，则会认为连接已经丢失并主动发出Close控制包，并释放该WebSocket对应的订阅及与其相关的计算资源。订阅释放后，上位应用需要重新发起新的订阅才能获得后续的订阅信息。<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;数据体采用JSON格式，UTF-8编码。<br>

#### &nbsp;&nbsp;&nbsp;&nbsp;3.2时间表达约定<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;数据内容中如果包含时间戳，均以毫秒精度的Unix时间表达，即约定的时间原点（UTC时区的1970年1月1日 00:00:00.000）与该时间戳所表达的时间之间的毫秒数（不考虑闰秒）。例如北京时间2021年7月1日上午8:00:00.000应表达为长整形数1625097600000<br>

<p style="color: red;">注意：<br>
1. PO2022需开启CacheSwitch.NeedSleepAfterQuery，同时修改ThreadSetting.HistoryMaxDegreeOfParallelism为1，以保证PO稳定性；<br>
2. CacheSwitch.NeedSleepAfterQuery适用于全局定SystemModel缓存，实时数据缓存，点位历史数据查询，告警历史查询；<br>
3. 非同服务器部署调用方需手动安装北向HTTPS证书；

## 四、功能说明
 
HaiLong Wu 
sesa669011@se.com 
 
## 1、认证与授权 


### 用户登录授权
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >Authentication</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意： <br> 
1.上位应用在调用北向接口以获取数据之前，应当先完成应用的身份认证（Authentication）并获得相应的数据访问授权（Authorization）。 <br> 
2.北向接口认证和授权上位应用的机制符合OAuth 2.0标准框架规范 （RFC6749）定义中客户端凭证模式（Client Credentials Grant）完成身份确认并获得访问令牌（Access Token）。 <br> 
3.上位应用应当在发起后续数据查询/订阅请求的时候附带上访问令牌，并在访问令牌过期后的后续请求之前完成重新认证授权的操作 <br>
4.不支持全数字用户名;
5.如需使用windows用户登录需将windows用户加入 Pso_Operators或Pso_Administrator权限用户组且windowsUser传参为true;</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >grant_type</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >客户端凭证模式必须为client_credentials</td>
<td >client_credentials</td>
</tr><tr>
<td >client_id</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >PO用户名</td>
<td >admin</td>
</tr><tr>
<td >client_secret</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >PO密码</td>
<td >admin</td>
</tr><tr>
<td >windows_user</td>
<td >boolean</td>
<td >Header</td>
<td >N</td>
<td >windows用户登录</td>
<td ></td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >access_token</td>
<td colspan="2">string</td>
<td colspan="3" >访问令牌</td>
</tr>
<tr>
<td >token_type</td>
<td colspan="2">string</td>
<td colspan="3" >Token类型</td>
</tr>
<tr>
<td >expires_in</td>
<td colspan="2">integer</td>
<td colspan="3" >过期时间</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

{
····"access_token": "eyJhbGciOiJIUzI1NiI...sInR5cCI6IkpXVCJ9",
····"token_type": "Bearer",
····"expires_in": 1800
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">429</td>
<td colspan="2">Too Many Requests</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr></table>

### 检查用户权限
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >HasAnyPrivilege</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意： <br> 
1.检查当前用户的权限组，假如传入权限组为空抛出异常，英文逗号分割 <br> 
2.假如不包含权限返回false <br> 
3.假如包含权限返回true <br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >privileges</td>
<td >string</td>
<td >Query</td>
<td >N</td>
<td >privilege group</td>
<td >1,2,3</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td ></td>
<td >eyJhbGciO...iJIUzI1NiIs</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

false

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr></table> 
## 2、查询功能接口 


### 获取活动告警列表
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetActiveAlarmsByFilter</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >通过告警点位的on属性判断点位活动状态；
注意： <br> 
1.当传递空设备、点位返回全部；<br> 
2.使用MemoryCache缓存机制；通过后台GlobalCacheTimer定时任务刷新数据。 <br> 
3.可通过CacheSwitch.RealTime配置缓存开启与否； <br> 
4.可通过Timer.GlobalTimer CRON表达式配置缓存刷新频率, 默认2s；  <br> 
5.PO2022需开启CacheSwitch.NeedSleepAfterQuery，同时修改ThreadSetting.HistoryMaxDegreeOfParallelism为1，以保证PO稳定性，</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >令牌</td>
<td ></td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····tags</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">点位集合</td>
</tr>
<tr>
<td >····equips</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">设备集合</td>
</tr>
<tr>
<td >····size</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">值数量</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"tags": [
········"c1.D3_F1B01_21_BA1%PLC_BatteryStatu"
····],
····"equips": [
········"c1.D3F1.SR02.Distribution.LowVoltageATS.D3_F1SR02_AT_01"
····],
····"size": 0
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····tag</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称</td>
</tr>
<tr>
<td >····val</td>
<td colspan="2">number</td>
<td colspan="3" >点位最后更新数值</td>
</tr>
<tr>
<td >····ts</td>
<td colspan="2">integer</td>
<td colspan="3" >点位最后更新时间</td>
</tr>
<tr>
<td >····quality</td>
<td colspan="2">string</td>
<td colspan="3" >点位数据质量通用实体</td>
</tr>
<tr>
<td >····category</td>
<td colspan="2">string</td>
<td colspan="3" >类别</td>
</tr>
<tr>
<td >····equip</td>
<td colspan="2">string</td>
<td colspan="3" >设备</td>
</tr>
<tr>
<td >····msg</td>
<td colspan="2">string</td>
<td colspan="3" >消息</td>
</tr>
<tr>
<td >····desc</td>
<td colspan="2">string</td>
<td colspan="3" >报警点位描述</td>
</tr>
<tr>
<td >····comment</td>
<td colspan="2">string</td>
<td colspan="3" >备注</td>
</tr>
<tr>
<td >····type</td>
<td colspan="2">string</td>
<td colspan="3" >报警类型</td>
</tr>
<tr>
<td >····priority</td>
<td colspan="2">string</td>
<td colspan="3" >优先级</td>
</tr>
<tr>
<td >····state</td>
<td colspan="2">number</td>
<td colspan="3" >告警状态</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"tag": "PLSDCluster.PLS_AdvOL_LogEng_Fail_Primary",
········"val": 1.0,
········"ts": 1656897191977,
········"quality": "Good|Bad|Uncertain",
········"category": "1001",
········"equip": "PLSDCluster.Memory_Device.OneLine",
········"msg": "PLS_AdvOneLine - Primary Logic Engine Status",
········"desc": "PLS_AdvOneLine - Primary Logic Engine Status",
········"comment": "",
········"type": "Advanced",
········"priority": "1",
········"state": 257.0
····}
]

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr></table>

### 获取缓存信息
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >Info</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >展示北向接口系统模型信息缓存状态；展示北向接口实时数据缓存状态；展示北向历史数据缓存命中区间；
注意： <br>
本接口为非对外功能服务接口，仅作为北向内部使用，不建议作为外部功能使用，且不保证功能后续兼容性 <br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >token令牌</td>
<td ></td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >cacheSwitchHierarchy</td>
<td colspan="2">boolean</td>
<td colspan="3" >设备树缓存开启标志</td>
</tr>
<tr>
<td >verifyHierarchy</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >hierarchyTreeCount</td>
<td colspan="2">integer</td>
<td colspan="3" >设备树数量</td>
</tr>
<tr>
<td >hierarchyTableCount</td>
<td colspan="2">integer</td>
<td colspan="3" >设备树表数量</td>
</tr>
<tr>
<td >equipmentCount</td>
<td colspan="2">integer</td>
<td colspan="3" >设备数量</td>
</tr>
<tr>
<td >variableTagCount</td>
<td colspan="2">integer</td>
<td colspan="3" >点位数量</td>
</tr>
<tr>
<td >alarmTagCount</td>
<td colspan="2">integer</td>
<td colspan="3" >告警点位数量</td>
</tr>
<tr>
<td >trendTagCount</td>
<td colspan="2">integer</td>
<td colspan="3" >趋势点数量</td>
</tr>
<tr>
<td >cacheSwitchRealTime</td>
<td colspan="2">boolean</td>
<td colspan="3" >实时缓存开关开启标志</td>
</tr>
<tr>
<td >variableTagDataCount</td>
<td colspan="2">integer</td>
<td colspan="3" >实时点位缓存数量</td>
</tr>
<tr>
<td >syncStartDate</td>
<td colspan="2">string</td>
<td colspan="3" >同步开始日期</td>
</tr>
<tr>
<td >syncedDate</td>
<td colspan="2">string</td>
<td colspan="3" >同步进度</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

{
····"cacheSwitchHierarchy": false,
····"verifyHierarchy": false,
····"hierarchyTreeCount": 0,
····"hierarchyTableCount": 0,
····"equipmentCount": 0,
····"variableTagCount": 0,
····"alarmTagCount": 0,
····"trendTagCount": 0,
····"cacheSwitchRealTime": false,
····"variableTagDataCount": 0,
····"syncStartDate": "string",
····"syncedDate": "string"
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr></table>

### 执行CiCode
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >Excute</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意： <br> 
1.可根据开发需求动态传入需要执行的CiCode代码并返回结果。
2.传入空代码会报异常，传入错误代码会返回空 <br> 
3.获取po启动时间代码：CitectInfo(General, General, 5) <br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td ></td>
<td >eyJhbGciO...iJIUzI1NiIs</td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >_</td>
<td >string</td>
<td >Body</td>
<td ></td>
<td colspan="2"></td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr></table>

### 获取报警历史数据
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetAlarmHistoryData</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意： <br> 
1.如果未在PO系统中定义的报警/事件，则无法获得报警/事件的历史。 <br> 
2.如果指定的点位/层级节点过多，或查询的时间段过长，为了避免长时间占用过多系统资源以及响应超时的情况，该接口会截取一部分记录进行返回并标注结果不完整（complete=false）。这并非是最佳设计，而是在功能、资源占用和复杂度之间的一种权衡。这种情况下，上位应用应该减少查询的点位/层级节点数量或缩小查询时间段已获得完整的历史报警/事件信息记录 <br>
3.过滤此事件发生的告警过滤state为Active的记录 <br> 
4.对于开关量报警，应用无法直接通过alarmState字段来标定报警状态，都是Expired <br> 
5.对于模拟量报警，应用可以通过alarmState字段来标定报警状态； <br> 
6.报警状态active|normal|acknowledged|reset； <br> 
7.PO2022需开启CacheSwitch.NeedSleepAfterQuery，同时修改ThreadSetting.HistoryMaxDegreeOfParallelism为1，以保证PO稳定性，<br> 
8.建议不要按时间段分批请求这样会造成数据索引耗时；</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >token</td>
<td ></td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····from</td>
<td >integer</td>
<td >Body</td>
<td >Y</td>
<td colspan="2">开始时间戳不能为空 毫秒值</td>
</tr>
<tr>
<td >····to</td>
<td >integer</td>
<td >Body</td>
<td >Y</td>
<td colspan="2">截至时间戳不能为空 毫秒值</td>
</tr>
<tr>
<td >····tags</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">需要查询的点位名称数组
tags与equips不能同时为空</td>
</tr>
<tr>
<td >····equips</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">需要查询的层级节点名称数组，支持通配符。例如：folder.*
tags与equips不能同时为空</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"from": 1659974400000,
····"to": 1660147200000,
····"tags": [
········"c1.D3_F1P01_TH_1\\PLC_Roomhumidity\\HALM",
········"c1.D4_F1IT01_IDEC05\\IDEC\\Emergency_start_Integrated"
····],
····"equips": "string"
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >complete</td>
<td colspan="2">boolean</td>
<td colspan="3" >表明返回的数据是否完整（完整的含义为：不存在报警/事件过多的情况）</td>
</tr>
<tr>
<td >events</td>
<td colspan="2">array</td>
<td colspan="3" >报警/事件信息对象列表</td>
</tr>
<tr>
<td >········tag</td>
<td colspan="2">string</td>
<td colspan="3" >报警点位</td>
</tr>
<tr>
<td >········category</td>
<td colspan="2">string</td>
<td colspan="3" >类别</td>
</tr>
<tr>
<td >········equip</td>
<td colspan="2">string</td>
<td colspan="3" >设备</td>
</tr>
<tr>
<td >········msg</td>
<td colspan="2">string</td>
<td colspan="3" >消息</td>
</tr>
<tr>
<td >········desc</td>
<td colspan="2">string</td>
<td colspan="3" >报警点位描述</td>
</tr>
<tr>
<td >········state</td>
<td colspan="2">string</td>
<td colspan="3" >状态</td>
</tr>
<tr>
<td >········ts</td>
<td colspan="2">integer</td>
<td colspan="3" >报警发生事件</td>
</tr>
<tr>
<td >········comment</td>
<td colspan="2">string</td>
<td colspan="3" >备注</td>
</tr>
<tr>
<td >········type</td>
<td colspan="2">string</td>
<td colspan="3" >报警类型</td>
</tr>
<tr>
<td >········alarmState</td>
<td colspan="2">string</td>
<td colspan="3" >报警状态</td>
</tr>
<tr>
<td >········value</td>
<td colspan="2">integer</td>
<td colspan="3" >告警值</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

{
····"complete": true,
····"events": [
········{
············"tag": "c3.D3_F3IT01_WTH_1\\PLC_H8010N2131_Temperature",
············"category": "1002",
············"equip": "c3.D3F3.IT01.Environment.TemperatureAndHumidity.D3_F3IT01_WTH_1",
············"msg": "WTH1室外温度",
············"desc": "WTH1室外温度",
············"state": "Active|Normal|Acknowledged|Reset",
············"ts": 1658731114000,
············"comment": "string",
············"type": "Analog",
············"alarmState": "Expired",
············"value": 0
········}
····]
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr></table>

### 获取系统层级
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetHierarchy</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >通过调用系统层级查询接口接口可获得PO系统中已经配置的逻辑层级节点（Equipment）结构树及层级节点中可能包含的点位列表信息。<br> 
注意： <br> 
1.每次调用该功能接口都需要完整遍历PO内部的层级、点位等信息，查询开销较大，上位应用应当在有绝对必要时调用该接口（例如上位应用启动时，或通过人工确认确知PO系统内部层级点表已发生变化后） <br> 
2.开启进程完整度验证"CacheSwitch:VerifyHierarchy": true,会校验集群的alm\trn\io进程运行状态；当集群之一以上进程运行则进行查询，全局缓存器同时使用此开关 <br>
3.Name过滤逻辑为StartWith;</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >Name</td>
<td >string</td>
<td >Query</td>
<td >N</td>
<td >Hierarchy节点名称,置空查询全部</td>
<td >PLSDCluster.High_Voltage</td>
</tr><tr>
<td >ShowTags</td>
<td >boolean</td>
<td >Query</td>
<td >N</td>
<td >是否显示Tag</td>
<td >False</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >token</td>
<td >eyJhbGciO...iJIUzI1NiIs</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····name</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称</td>
</tr>
<tr>
<td >····subordinates</td>
<td colspan="2">array</td>
<td colspan="3" >该层级节点下的直接子层级节点列表</td>
</tr>
<tr>
<td >········name</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称</td>
</tr>
<tr>
<td >········subordinates</td>
<td colspan="2">array</td>
<td colspan="3" >该层级节点下的直接子层级节点列表</td>
</tr>
<tr>
<td >········tags</td>
<td colspan="2">array</td>
<td colspan="3" >该层级节点下的直接点位列表</td>
</tr>
<tr>
<td >················name</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称</td>
</tr>
<tr>
<td >················itemName</td>
<td colspan="2">string</td>
<td colspan="3" >点位项名称，往往用于表达该点位的物理含义</td>
</tr>
<tr>
<td >················extensions</td>
<td colspan="2">string</td>
<td colspan="3" >点位类型：
ALM：表明该点位有与之关联的报警
TRN：表明该点位有与之关联的历史趋势</td>
</tr>
<tr>
<td >················unit</td>
<td colspan="2">string</td>
<td colspan="3" >工程单位</td>
</tr>
<tr>
<td >················description</td>
<td colspan="2">string</td>
<td colspan="3" >点位说明</td>
</tr>
<tr>
<td >····tags</td>
<td colspan="2">array</td>
<td colspan="3" >该层级节点下的直接点位列表</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"name": "High_Voltage",
········"subordinates": [
············{
················"name": "High_Voltage",
················"subordinates": [],
················"tags": [
····················{
························"name": "PLSDCluster.S33K_AB_CPL\\A27_PTUV2\\Op\\dchg",
························"itemName": "Alm_Prot27S_2",
························"extensions": "ALM",
························"unit": "",
························"description": "Protection 27/27S unit 2"
····················}
················]
············}
········],
········"tags": []
····}
]

</td>
</tr></table>

### 检查集群状态
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >PingAsync</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意： <br> 
1.CSV 是检查PO时连带检查指定集群的趋势服务是不是有活着的实例。 <br> 
2.将删除空集群名称，并自动修剪集群名称。多个集群英文逗号分割符。 <br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td ></td>
<td >eyJhbGciO...iJIUzI1NiIs</td>
</tr><tr>
<td >cluster-csv</td>
<td >string</td>
<td >Query</td>
<td >N</td>
<td >集群名称</td>
<td >c1,c2</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >powerOperationTimestamp</td>
<td colspan="2">string</td>
<td colspan="3" >The Power Operation Timestamp.</td>
</tr>
<tr>
<td >northboundApiTimestamp</td>
<td colspan="2">string</td>
<td colspan="3" >The Northbound Api Timestamp.</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

{
····"powerOperationTimestamp": "2023-04-27T15:28:27.8547748+08:00",
····"northboundApiTimestamp": "2023-04-27T15:28:27.8547748+08:00"
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr></table>

### 获取PO系统时间
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetCurrentDateTime</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意： <br> 
1.返回秒级时间戳10位 <br>
2.连接多个PO形成集群返回北向所连接PO时间戳</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td ></td>
<td ></td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr></table>

### 获取点位基本信息
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetTagsByFilter</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意 <br> 
1.默认先从缓存中获取，当缓存不存在从ctapi中查询 <br> 
2.包括变量、趋势、告警 <br> 
3.传空列表获取全部 <br>
4.TagPrefix为空则无法返回TagCategory <br>
5.需要调用方去重</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td ></td>
<td ></td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····tags</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">需要查询的点位名称数组</td>
</tr>
<tr>
<td >····equips</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">需要查询的层级节点名称数组，支持通配符。例如：folder.*</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"tags": [
········"c1.D3_F1P01_TH_1\\PLC_Roomhumidity\\HALM",
········"c1.D4_F1IT01_IDEC05\\IDEC\\Emergency_start_Integrated"
····],
····"equips": [
········"PLSDCluster.pm8000"
····]
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····cluster</td>
<td colspan="2">string</td>
<td colspan="3" >集群</td>
</tr>
<tr>
<td >····equipmentName</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称</td>
</tr>
<tr>
<td >····equipmentFullName</td>
<td colspan="2">string</td>
<td colspan="3" >设备全称(带集群名称)</td>
</tr>
<tr>
<td >····tagName</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称
TrendTag的点位名称读取的表属性是TAG/NAME
AlarmTag的点位名称读取的表属性是NAME</td>
</tr>
<tr>
<td >····tagFullName</td>
<td colspan="2">string</td>
<td colspan="3" >点位全称(带集群名称)</td>
</tr>
<tr>
<td >····deviceName</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称
设置Equipment:=Cluster+Equipment
未设置Equipment:=DeviceSignature</td>
</tr>
<tr>
<td >····comment</td>
<td colspan="2">string</td>
<td colspan="3" >备注</td>
</tr>
<tr>
<td >····tagCategory</td>
<td colspan="2">string</td>
<td colspan="3" >点位类别</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"cluster": "PLSDCluster",
········"equipmentName": "Medium_Voltage.Transfers.TRF6_To_D",
········"equipmentFullName": "PLSDCluster.Medium_Voltage.Transfers.TRF6_To_D",
········"tagName": "S6K_D_TRF6\\A81L_PTUF1\\Op\\dchg",
········"tagFullName": "PLSDCluster.S6K_D_TRF6\\A81L_PTUF1\\Op\\dchg",
········"deviceName": "",
········"comment": "",
········"tagCategory": "\\A81L_PTUF1\\Op\\dchg"
····}
]

</td>
</tr></table>

### 获取告警基本信息
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetAlarmsByFilter</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意 <br> 
1.传空列表获取全部 <br>
2.传入了错误点位或设备，过滤条件错误返回400 <br>
3.传如正确点位设备，根据条件返回相应告警点基本信息 <br>
4.没有缓存或者查询点位为空返回204</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td ></td>
<td >eyJhbGciO...iJIUzI1NiIs</td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····tags</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">需要查询的点位名称数组</td>
</tr>
<tr>
<td >····equips</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">需要查询的层级节点名称数组，支持通配符。例如：folder.*</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"tags": [
········"c1.D3_F1P01_TH_1\\PLC_Roomhumidity\\HALM",
········"c1.D4_F1IT01_IDEC05\\IDEC\\Emergency_start_Integrated"
····],
····"equips": [
········"PLSDCluster.pm8000"
····]
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····cluster</td>
<td colspan="2">string</td>
<td colspan="3" >集群</td>
</tr>
<tr>
<td >····equipmentName</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称</td>
</tr>
<tr>
<td >····equipmentFullName</td>
<td colspan="2">string</td>
<td colspan="3" >设备全称(带集群名称)</td>
</tr>
<tr>
<td >····tagName</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称
TrendTag的点位名称读取的表属性是TAG/NAME
AlarmTag的点位名称读取的表属性是NAME</td>
</tr>
<tr>
<td >····tagFullName</td>
<td colspan="2">string</td>
<td colspan="3" >点位全称(带集群名称)</td>
</tr>
<tr>
<td >····deviceName</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称
设置Equipment:=Cluster+Equipment
未设置Equipment:=DeviceSignature</td>
</tr>
<tr>
<td >····comment</td>
<td colspan="2">string</td>
<td colspan="3" >备注</td>
</tr>
<tr>
<td >····tagCategory</td>
<td colspan="2">string</td>
<td colspan="3" >点位类别</td>
</tr>
<tr>
<td >····name</td>
<td colspan="2">string</td>
<td colspan="3" >报警点位名称</td>
</tr>
<tr>
<td >····category</td>
<td colspan="2">string</td>
<td colspan="3" >分类</td>
</tr>
<tr>
<td >····alarmType</td>
<td colspan="2">string</td>
<td colspan="3" >报警类型,详见po systemmodel alarms</td>
</tr>
<tr>
<td >····priority</td>
<td colspan="2">string</td>
<td colspan="3" >优先级</td>
</tr>
<tr>
<td >····highHigh</td>
<td colspan="2">string</td>
<td colspan="3" >高高限</td>
</tr>
<tr>
<td >····high</td>
<td colspan="2">string</td>
<td colspan="3" >高限</td>
</tr>
<tr>
<td >····low</td>
<td colspan="2">string</td>
<td colspan="3" >低</td>
</tr>
<tr>
<td >····lowLow</td>
<td colspan="2">string</td>
<td colspan="3" >低低</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"cluster": "PLSDCluster",
········"equipmentName": "Medium_Voltage.Transfers.TRF6_To_D",
········"equipmentFullName": "PLSDCluster.Medium_Voltage.Transfers.TRF6_To_D",
········"tagName": "S6K_D_TRF6\\A81L_PTUF1\\Op\\dchg",
········"tagFullName": "PLSDCluster.S6K_D_TRF6\\A81L_PTUF1\\Op\\dchg",
········"deviceName": "",
········"comment": "",
········"tagCategory": "\\A81L_PTUF1\\Op\\dchg",
········"name": "Protection 81L unit 1",
········"category": "1001",
········"alarmType": "Digital",
········"priority": "1",
········"highHigh": "100",
········"high": "50",
········"low": "",
········"lowLow": ""
····}
]

</td>
</tr></table>

### 获取点位实时数据
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetRealTimeTagDataAsync</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意：<br> 
1.通过实时数据查询接口可获得PO系统中各点位的最后更新数值。此接口可以接受按层级节点或按点位查询。如果是按照包含通配符的层级节点查询，此接口会返回此层级节点所包含的所有点位值，并递归查询其所有子层级节点所包含的所有点位值，但是不同的层级节点的点位值会平铺在返回的数组中，并不会保留原有的层级结构。<br>
2.由于点位值的更新本质上是基于轮询机制的，从真实世界物理值发生变化到更新至其数字孪生副本之间存在时间差，因此无论是否禁用缓存机制与否，都无法消除由于上述时间差导致的真实值与通过该接口获得的测量值之间的差异。<br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >token</td>
<td ></td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····tags</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">点位集合</td>
</tr>
<tr>
<td >····equips</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">设备集合</td>
</tr>
<tr>
<td >····size</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">值数量</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"tags": [
········"c1.D3_F1B01_21_BA1%PLC_BatteryStatu"
····],
····"equips": [
········"c1.D3F1.SR02.Distribution.LowVoltageATS.D3_F1SR02_AT_01"
····],
····"size": 0
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····tag</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称</td>
</tr>
<tr>
<td >····equipName</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称</td>
</tr>
<tr>
<td >····val</td>
<td colspan="2">number</td>
<td colspan="3" >点位最后更新数值</td>
</tr>
<tr>
<td >····ts</td>
<td colspan="2">integer</td>
<td colspan="3" >点位最后更新时间</td>
</tr>
<tr>
<td >····quality</td>
<td colspan="2">string</td>
<td colspan="3" >点位数据质量通用实体</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"tag": "PLSDCluster.pm8000\\LPHD1\\EEHealth.on",
········"equipName": "PLSDCluster.pm8000",
········"val": 1.0,
········"ts": 1656897191977,
········"quality": "Good|Bad|Uncertain"
····}
]

</td>
</tr></table>

### 获取波形实时数据
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetWaveformDataAsync</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意：<br> 
1.通过波形实时数据查询接口可获得PO系统中各点位的波形数值。此接口可以接受按点位以及该波形长度查询。<br>
2.由于点位值的更新本质上是基于轮询机制的，从真实世界物理值发生变化到更新至其数字孪生副本之间存在时间差，因此无论是否禁用缓存机制与否，都无法消除由于上述时间差导致的真实值与通过该接口获得的测量值之间的差异。<br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >token</td>
<td ></td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····tag</td>
<td >string</td>
<td >Body</td>
<td ></td>
<td colspan="2">需要查询的点位名称</td>
</tr>
<tr>
<td >····length</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">波形长度</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"tag": "c1.D3_F1P01_TH_1\\PLC_Roomhumidity\\HALM ",
····"length": 100
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····tag</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称</td>
</tr>
<tr>
<td >····equipName</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称</td>
</tr>
<tr>
<td >····val</td>
<td colspan="2">number</td>
<td colspan="3" >点位最后更新数值</td>
</tr>
<tr>
<td >····ts</td>
<td colspan="2">integer</td>
<td colspan="3" >点位最后更新时间</td>
</tr>
<tr>
<td >····quality</td>
<td colspan="2">string</td>
<td colspan="3" >点位数据质量通用实体</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"tag": "PLSDCluster.pm8000\\LPHD1\\EEHealth.on",
········"equipName": "PLSDCluster.pm8000",
········"val": 1.0,
········"ts": 1656897191977,
········"quality": "Good|Bad|Uncertain"
····}
]

</td>
</tr></table>

### 获取点位历史数据
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetTagHistoryData</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >通过历史数据查询接口可获得PO系统中已经定义的趋势（Trend）数据。
注意：<br> 
1.如果相关点位未在PO系统中定义趋势，则无法获得点位的历史。<br> 
2.历史数据的采样周期由PO中的定义的趋势所确定。<br> 
3.如果指定的点位过多或查询的时间段过长，为了避免长时间占用过多系统资源以及响应超时的情况，该接口会截取一部分记录进行返回并标注结果不完整（complete=false）。这并非是最佳设计，而是在功能、资源占用和复杂度之间的一种权衡。这种情况下，上位应用应该减少查询的点位数量或缩小查询时间段已获得完整的历史数据记录。<br> 
4.添加是否使用缓存功能，主要针对大量数据做的查询优化，使用前应确认历史数据缓存状态可在18101服务端口中查看同步状态<br> 
5.在启动同步TrendDataSyncSetting:EnableTrendDataSyncProducerTimer情况下，请求的开始时间大于同步开始时间；请求的截止时间小于同步进度时间-1天则表示没有穿透缓存，可以从cacheInfo接口查看缓存范围<br> 
6.true,强制使用缓存不论是否完整，<br> 
  false,强制不使用缓存不论是否完整，<br> 
  null，开启了同步且没有缓存穿透，生产情况<br> 
7.重采样中ts时间戳不准确<br>
8.返回complete标识数据是否完整，此项是保证接口响应速度和提示用户就大量数据需调整请求数量的配置；<br>
9.默认50000（QuerySetting.TagHistoryQueryCount）返回数据量，超出设置无法保证响应速度及PO稳定；<br> 
10.对于对性能有要求的调用请开启北向缓存服务，并【强制使用缓存】useCache传true,并可接受数据延迟延迟（具体延迟时间跟点位量相关）<br>
11.采样周期不应大于请求起止时间；CtApi只会返回离1970年最近的最值结果当存在多个最值有可能发生最值不在起止时间内从而遗漏；<br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >token</td>
<td >4457971f-66c5-4743-8e14-e24627609aff</td>
</tr><tr>
<td >useCache</td>
<td >boolean</td>
<td >Query</td>
<td >N</td>
<td >是否使用缓存</td>
<td ></td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····from</td>
<td >integer</td>
<td >Body</td>
<td >Y</td>
<td colspan="2">起始时间</td>
</tr>
<tr>
<td >····to</td>
<td >integer</td>
<td >Body</td>
<td >Y</td>
<td colspan="2">截止时间</td>
</tr>
<tr>
<td >····tags</td>
<td >array</td>
<td >Body</td>
<td >Y</td>
<td colspan="2">需要查询的点位名称数组</td>
</tr>
<tr>
<td >····resample</td>
<td >object</td>
<td >Body</td>
<td ></td>
<td colspan="2">重采样指令,</td>
</tr>
<tr>
<td >············period</td>
<td >object</td>
<td >Body</td>
<td ></td>
<td colspan="2">重采样周期</td>
</tr>
<tr>
<td >····················months</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">重采样周期月数</td>
</tr>
<tr>
<td >····················days</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">重采样周期天数</td>
</tr>
<tr>
<td >····················hours</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">重采样周期小时数</td>
</tr>
<tr>
<td >····················minutes</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">重采样周期分钟数</td>
</tr>
<tr>
<td >····················seconds</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">重采样周期秒数</td>
</tr>
<tr>
<td >····················reSample</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">重采样秒数</td>
</tr>
<tr>
<td >············Condense_method</td>
<td >string</td>
<td >Body</td>
<td ></td>
<td colspan="2">数据压缩方法, MEAN：取均值 ,MIN：取最小值, MAX：取最大值 ,NEW：取时间轴上最新值</td>
</tr>
<tr>
<td >············Stretch_method</td>
<td >string</td>
<td >Body</td>
<td ></td>
<td colspan="2">数据填充方法, STEP：阶跃保持, RATIO：比例填充 ,NO：不填充</td>
</tr>
<tr>
<td >············timestamp_options</td>
<td >string</td>
<td >Body</td>
<td ></td>
<td colspan="2">0 - Returns the mean timestamp for all of the samples in the sample period.
33554432 - Returns the actual timestamp of the minimum/maximum/newest sample (depending on the current condense method setting). A condense method of "mean" will still return the mean timestamp for all of the samples in the sample period.</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"from": 1609430400000,
····"to": 1640966400000,
····"tags": [
········"c1.D3_F1B01_21_BA1%PLC_BatteryStatu"
····],
····"resample": {
········"period": {
············"months": 0,
············"days": 0,
············"hours": 1,
············"minutes": 0,
············"seconds": 0,
············"reSample": 0
········},
········"Condense_method": "MEAN",
········"Stretch_method": "NO",
········"timestamp_options": "NO"
····}
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >complete</td>
<td colspan="2">boolean</td>
<td colspan="3" >表明返回的数据是否完整（完整的含义为：不存在数据点过多或查询时间段过长的情况）</td>
</tr>
<tr>
<td >tags</td>
<td colspan="2">array</td>
<td colspan="3" >点位历史数据对象列表</td>
</tr>
<tr>
<td >········tag</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称</td>
</tr>
<tr>
<td >········equipComment</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称</td>
</tr>
<tr>
<td >········records</td>
<td colspan="2">array</td>
<td colspan="3" >点位结果集</td>
</tr>
<tr>
<td >············val</td>
<td colspan="2">number</td>
<td colspan="3" >点位最后更新数值</td>
</tr>
<tr>
<td >············ts</td>
<td colspan="2">integer</td>
<td colspan="3" >点位最后更新时间</td>
</tr>
<tr>
<td >············quality</td>
<td colspan="2">string</td>
<td colspan="3" >点位数据质量通用实体</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

{
····"complete": false,
····"tags": [
········{
············"tag": "c3.D3_F3IT01_WTH_1\\PLC_H8010N2131_Temperature",
············"equipComment": "",
············"records": [
················{
····················"val": 0.0,
····················"ts": 1658731140000,
····················"quality": "Good|Bad|Uncertain"
················}
············]
········}
····]
}

</td>
</tr></table>

### 获取点位指标结果
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetTagIndicatorData</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意： <br>
1.多用于查询最大值最小值及其发生时间 <br>
2.实现原理位获取时间段内所有历史数据计算最大值最小值<br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >token</td>
<td >4457971f-66c5-4743-8e14-e24627609aff</td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····from</td>
<td >integer</td>
<td >Body</td>
<td >Y</td>
<td colspan="2">起始时间</td>
</tr>
<tr>
<td >····to</td>
<td >integer</td>
<td >Body</td>
<td >Y</td>
<td colspan="2">截止时间</td>
</tr>
<tr>
<td >····tags</td>
<td >array</td>
<td >Body</td>
<td >Y</td>
<td colspan="2">需要查询的点位名称数组</td>
</tr>
<tr>
<td >····types</td>
<td >array</td>
<td >Body</td>
<td >Y</td>
<td colspan="2">查询指标类型
MIN,MAX</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"from": 1658731107000,
····"to": 1658731167000,
····"tags": [
········"c3.D3_F3IT01_WTH_1\\PLC_H8010N2131_Temperature"
····],
····"types": [
········"MIN",
········"MAX"
····]
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····tag</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称</td>
</tr>
<tr>
<td >····indicators</td>
<td colspan="2">array</td>
<td colspan="3" >点位结果集</td>
</tr>
<tr>
<td >········val</td>
<td colspan="2">number</td>
<td colspan="3" >点位最后更新数值</td>
</tr>
<tr>
<td >········ts</td>
<td colspan="2">integer</td>
<td colspan="3" >点位最后更新时间</td>
</tr>
<tr>
<td >········type</td>
<td colspan="2">string</td>
<td colspan="3" >点位指标类型</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"tag": "string",
········"indicators": [
············{
················"val": 0.0,
················"ts": 1658731140000,
················"type": "MAX"
············}
········]
····}
]

</td>
</tr></table> 
## 3、故障录波接口 


### 获取系统发生告警
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetDevicesAlarmsByTimeRange</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  > 注意： <br>
1.使用配置文件用户名密码进行WebHmi用户登录授权； <br>
2.依赖配置CtApi连接电脑的WebHmi,当未安装WebHmi无法登录，当WebHmi未启动无法登录；无法获取服务；</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td ></td>
<td ></td>
</tr><tr>
<td >startDate</td>
<td >integer</td>
<td >Query</td>
<td >N</td>
<td >开始时间</td>
<td >1683339464000</td>
</tr><tr>
<td >endDate</td>
<td >integer</td>
<td >Query</td>
<td >N</td>
<td >截止时间</td>
<td >1686017864000</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····id</td>
<td colspan="2">string</td>
<td colspan="3" >告警id</td>
</tr>
<tr>
<td >····alarmDefinitionId</td>
<td colspan="2">string</td>
<td colspan="3" >告警定义</td>
</tr>
<tr>
<td >····incidentId</td>
<td colspan="2">string</td>
<td colspan="3" >事故id</td>
</tr>
<tr>
<td >····type</td>
<td colspan="2">string</td>
<td colspan="3" >告警type</td>
</tr>
<tr>
<td >····source</td>
<td colspan="2">object</td>
<td colspan="3" >告警设备</td>
</tr>
<tr>
<td >········sourceId</td>
<td colspan="2">string</td>
<td colspan="3" >告警id</td>
</tr>
<tr>
<td >········displayName</td>
<td colspan="2">string</td>
<td colspan="3" >显示名</td>
</tr>
<tr>
<td >········systemName</td>
<td colspan="2">string</td>
<td colspan="3" >系统名</td>
</tr>
<tr>
<td >········ianaTimeZoneName</td>
<td colspan="2">string</td>
<td colspan="3" >时区</td>
</tr>
<tr>
<td >····whatSummaryText</td>
<td colspan="2">string</td>
<td colspan="3" >总结</td>
</tr>
<tr>
<td >····whatDetailText</td>
<td colspan="2">string</td>
<td colspan="3" >详情</td>
</tr>
<tr>
<td >····startTimeUtc</td>
<td colspan="2">string</td>
<td colspan="3" >开始utc时间</td>
</tr>
<tr>
<td >····endTimeUtc</td>
<td colspan="2">string</td>
<td colspan="3" >截至utc时间</td>
</tr>
<tr>
<td >····durationMs</td>
<td colspan="2">number</td>
<td colspan="3" >持续毫秒</td>
</tr>
<tr>
<td >····priority</td>
<td colspan="2">number</td>
<td colspan="3" >优先级</td>
</tr>
<tr>
<td >····rawEvents</td>
<td colspan="2">array</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····pqEventId</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····dddDirection</td>
<td colspan="2">number</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····dddConfidence</td>
<td colspan="2">number</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····isActive</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····isUnacknowledged</td>
<td colspan="2">boolean</td>
<td colspan="3" >是否应答</td>
</tr>
<tr>
<td >····loadChangePercent</td>
<td colspan="2">number</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····timeQuality</td>
<td colspan="2">number</td>
<td colspan="3" >时序是质量</td>
</tr>
<tr>
<td >····timeQualityMicroseconds</td>
<td colspan="2">number</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····waveformCharacteristics</td>
<td colspan="2">array</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····lastModifiedUtc</td>
<td colspan="2">string</td>
<td colspan="3" >最后修改utc时间</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"id": "string",
········"alarmDefinitionId": "string",
········"incidentId": "string",
········"type": "string",
········"source": {
············"sourceId": "string",
············"displayName": "string",
············"systemName": "string",
············"ianaTimeZoneName": "string"
········},
········"whatSummaryText": "string",
········"whatDetailText": "string",
········"startTimeUtc": "string",
········"endTimeUtc": "string",
········"durationMs": 0.0,
········"priority": 0.0,
········"rawEvents": "string",
········"pqEventId": "string",
········"dddDirection": 0.0,
········"dddConfidence": 0.0,
········"isActive": false,
········"isUnacknowledged": false,
········"loadChangePercent": 0.0,
········"timeQuality": 0.0,
········"timeQualityMicroseconds": 0.0,
········"waveformCharacteristics": "string",
········"lastModifiedUtc": "string"
····}
]

</td>
</tr></table>

### 获取告警关联故障录波
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetWaveformIdByAlarmId</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  > 根据传递的报警id查询关联故障录波id列表，并根据分页规则进行数据分批查询，当pageSize和pageNumber为0则返回全部数据
 注意： <br>
1.使用配置文件用户名密码进行WebHmi用户登录授权； <br>
2.依赖配置CtApi连接电脑的WebHmi,当未安装WebHmi无法登录，当WebHmi未启动无法登录；无法获取服务；</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >令牌</td>
<td ></td>
</tr><tr>
<td >id</td>
<td >string</td>
<td >Path</td>
<td >Y</td>
<td >告警id</td>
<td >PLSDCluster.pm8000_PQ_SagSwell_SyncEvent_637956597363530000</td>
</tr><tr>
<td >pageSize</td>
<td >integer</td>
<td >Query</td>
<td >N</td>
<td >分页大小</td>
<td >0</td>
</tr><tr>
<td >pageNumber</td>
<td >integer</td>
<td >Query</td>
<td >N</td>
<td >页码</td>
<td >0</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····startPQEvent</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····endPQEvent</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····isPQEventOfInterest</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····source</td>
<td colspan="2">object</td>
<td colspan="3" >告警设备</td>
</tr>
<tr>
<td >········sourceId</td>
<td colspan="2">string</td>
<td colspan="3" >告警id</td>
</tr>
<tr>
<td >········displayName</td>
<td colspan="2">string</td>
<td colspan="3" >显示名</td>
</tr>
<tr>
<td >········systemName</td>
<td colspan="2">string</td>
<td colspan="3" >系统名</td>
</tr>
<tr>
<td >········ianaTimeZoneName</td>
<td colspan="2">string</td>
<td colspan="3" >时区</td>
</tr>
<tr>
<td >····series</td>
<td colspan="2">array</td>
<td colspan="3" >数据序列</td>
</tr>
<tr>
<td >········channelLabel</td>
<td colspan="2">string</td>
<td colspan="3" >通道标签</td>
</tr>
<tr>
<td >········channelLabelInvariant</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >········waveformId</td>
<td colspan="2">string</td>
<td colspan="3" >故障录波id</td>
</tr>
<tr>
<td >········phase</td>
<td colspan="2">number</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >········dataArray</td>
<td colspan="2">array</td>
<td colspan="3" >故障录波数据数组</td>
</tr>
<tr>
<td >····timeOfFirstTriggerUtc</td>
<td colspan="2">string</td>
<td colspan="3" >首次触发utc时间</td>
</tr>
<tr>
<td >····timeOfFirstPointUtc</td>
<td colspan="2">string</td>
<td colspan="3" >首次点utc时间</td>
</tr>
<tr>
<td >····timeOfLastPointUtc</td>
<td colspan="2">string</td>
<td colspan="3" >末次点utc时间</td>
</tr>
<tr>
<td >····timestampsOffsetFromFirstPoint</td>
<td colspan="2">boolean</td>
<td colspan="3" >距首次点时间差</td>
</tr>
<tr>
<td >····samplingFrequency</td>
<td colspan="2">number</td>
<td colspan="3" >采样频率</td>
</tr>
<tr>
<td >····lineFrequency</td>
<td colspan="2">number</td>
<td colspan="3" >线频率</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"startPQEvent": false,
········"endPQEvent": false,
········"isPQEventOfInterest": false,
········"source": {
············"sourceId": "string",
············"displayName": "string",
············"systemName": "string",
············"ianaTimeZoneName": "string"
········},
········"series": [
············{
················"channelLabel": "string",
················"channelLabelInvariant": "string",
················"waveformId": "string",
················"phase": 0.0,
················"dataArray": "string"
············}
········],
········"timeOfFirstTriggerUtc": "string",
········"timeOfFirstPointUtc": "string",
········"timeOfLastPointUtc": "string",
········"timestampsOffsetFromFirstPoint": false,
········"samplingFrequency": 0.0,
········"lineFrequency": 0.0
····}
]

</td>
</tr></table>

### 获取障录波详情
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetSeriesByWaveformId</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >根据传递的障录波id查询关联故障录波详情，并根据分页规则进行数据分批查询，当pageSize和pageNumber为0则返回全部数据
注意： <br>
1.使用配置文件用户名密码进行WebHmi用户登录授权； <br>
2.依赖配置CtApi连接电脑的WebHmi,当未安装WebHmi无法登录，当WebHmi未启动无法登录；无法获取服务；</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >令牌</td>
<td ></td>
</tr><tr>
<td >id</td>
<td >string</td>
<td >Path</td>
<td >Y</td>
<td ></td>
<td >PLSDCluster.pm8k|PLSDCluster.pm8k|Wfm_Tg_Mg_Sg_Sw_00000000001660071807_0000000931utc_16.cfg|A|1</td>
</tr><tr>
<td >pageSize</td>
<td >integer</td>
<td >Query</td>
<td >N</td>
<td >分页大小</td>
<td >0</td>
</tr><tr>
<td >pageNumber</td>
<td >integer</td>
<td >Query</td>
<td >N</td>
<td >页码</td>
<td >0</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····startPQEvent</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····endPQEvent</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····isPQEventOfInterest</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >····source</td>
<td colspan="2">object</td>
<td colspan="3" >告警设备</td>
</tr>
<tr>
<td >········sourceId</td>
<td colspan="2">string</td>
<td colspan="3" >告警id</td>
</tr>
<tr>
<td >········displayName</td>
<td colspan="2">string</td>
<td colspan="3" >显示名</td>
</tr>
<tr>
<td >········systemName</td>
<td colspan="2">string</td>
<td colspan="3" >系统名</td>
</tr>
<tr>
<td >········ianaTimeZoneName</td>
<td colspan="2">string</td>
<td colspan="3" >时区</td>
</tr>
<tr>
<td >····series</td>
<td colspan="2">array</td>
<td colspan="3" >数据序列</td>
</tr>
<tr>
<td >········channelLabel</td>
<td colspan="2">string</td>
<td colspan="3" >通道标签</td>
</tr>
<tr>
<td >········channelLabelInvariant</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >········waveformId</td>
<td colspan="2">string</td>
<td colspan="3" >故障录波id</td>
</tr>
<tr>
<td >········phase</td>
<td colspan="2">number</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >········dataArray</td>
<td colspan="2">array</td>
<td colspan="3" >故障录波数据数组</td>
</tr>
<tr>
<td >····timeOfFirstTriggerUtc</td>
<td colspan="2">string</td>
<td colspan="3" >首次触发utc时间</td>
</tr>
<tr>
<td >····timeOfFirstPointUtc</td>
<td colspan="2">string</td>
<td colspan="3" >首次点utc时间</td>
</tr>
<tr>
<td >····timeOfLastPointUtc</td>
<td colspan="2">string</td>
<td colspan="3" >末次点utc时间</td>
</tr>
<tr>
<td >····timestampsOffsetFromFirstPoint</td>
<td colspan="2">boolean</td>
<td colspan="3" >距首次点时间差</td>
</tr>
<tr>
<td >····samplingFrequency</td>
<td colspan="2">number</td>
<td colspan="3" >采样频率</td>
</tr>
<tr>
<td >····lineFrequency</td>
<td colspan="2">number</td>
<td colspan="3" >线频率</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"startPQEvent": false,
········"endPQEvent": false,
········"isPQEventOfInterest": false,
········"source": {
············"sourceId": "string",
············"displayName": "string",
············"systemName": "string",
············"ianaTimeZoneName": "string"
········},
········"series": [
············{
················"channelLabel": "string",
················"channelLabelInvariant": "string",
················"waveformId": "string",
················"phase": 0.0,
················"dataArray": "string"
············}
········],
········"timeOfFirstTriggerUtc": "string",
········"timeOfFirstPointUtc": "string",
········"timeOfLastPointUtc": "string",
········"timestampsOffsetFromFirstPoint": false,
········"samplingFrequency": 0.0,
········"lineFrequency": 0.0
····}
]

</td>
</tr></table> 
## 4、订阅功能接口 


### 点位数据变化订阅
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >RequestTagSubscriptionByCov</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >通过实时数据订阅接口可从PO系统中根据订阅点位和订阅策略组成的订阅条件获得实时点位数据。订阅策略分为点位值变化触发和定时触发两种。其中点位值变化触发策略允许通过变化阈值的类型和数值实现对死区的模拟；定时触发订阅则忽略点位值的变化与否，仅通过预定义的时间间隔推送最新的点位值。

注意： <br>
1.对于点位值变化触发策略来说，只要点位值的质量（Good/Bad）发生变化，则必然会推送该点位的最新值与质量，无论该点位值是否超过了订阅中对该点位的死区定义。<br>
2.由于点位值的更新本质上是基于轮询机制而存在客观存在的系统采样周期，如果真实世界中的物理值的变化频率高于系统既有的采样频率的两倍，则可能导致点位值变化订阅推送未能反映应有的点位变化信息而丢失关键数据（详情可查阅奈奎斯特-香农采样定律）。例如：PO与下位系统之间通过Modbus协议进行通讯，虽然轮询周期为50毫秒（忽略模数转换的时间与发起策略），但由于通过一个北向接口实例订阅的点位数量为50万点，该北向接口完成一次50万点轮询的时间为60秒，在此种采样周期下（50毫秒被60秒吸收为60秒），一个以60秒为周期的正弦信号由于欠采样，会被误读为恒定值而丧失关键信息。因此对于快变量的采样，需要单独安排独立的PO客户端-北向接口组合以保证其采样频率处于可接受的范围内。</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >header中access_token</td>
<td >eyJhbGciO...iJIUzI1NiIs</td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····tag</td>
<td >string</td>
<td >Body</td>
<td ></td>
<td colspan="2">需要订阅的点位名称</td>
</tr>
<tr>
<td >····type</td>
<td >string</td>
<td >Body</td>
<td ></td>
<td colspan="2">点位值变化阈值类型：
ABS：变化绝对值
PCT：变化百分比
默认值：ABS</td>
</tr>
<tr>
<td >····upper</td>
<td >number</td>
<td >Body</td>
<td ></td>
<td colspan="2">上阈值
默认值：0</td>
</tr>
<tr>
<td >····lower</td>
<td >number</td>
<td >Body</td>
<td ></td>
<td colspan="2">下阈值
默认值：与上阈值相同</td>
</tr>
<tr>
<td >····interval</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">点位值订阅时间间隔，单位：秒
默认值：900（15分钟）</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

[
····{
········"tag": "c1.D3_F1P01_TH_1\\PLC_Roomhumidity\\HALM",
········"type": "ABS",
········"upper": 0.0,
········"lower": 0.0,
········"interval": 10
····}
]

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >id</td>
<td colspan="2">string</td>
<td colspan="3" >The Subscription Id</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

{
····"id": "string"
}

</td>
</tr></table>

### 点位数据变化推送
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >ConnectCovTagSubscriptionAsync</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >请求示例：
ws://pso-api-server.com/subscriptions/tags/values/latest-by-cov/1e455915-e4fc-419a-8cd8-2cbf6c06b0d9
当WebSocket连接建立后，北向接口将会推送订阅中的所有点位的最新已知数据。
当既定订阅策略中的条件被满足时，北向接口将会通过已经建立好的WebSocket连接推送点位数据。 <br>
注意： <br>
1.基于RtDataCovTimer定时任务，查询间隔见[Timer]:[ChangeValueTimer]默认配置5s; <br>
2.当缓存开启从缓存查询，当缓存关闭从CtApi查询；<br>
3.通过deadBandValue(最后记录值)与当前值依据TagSubscriptionRequest Type配置(ABS：变化绝对值PCT：变化百分比)</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >id</td>
<td >string</td>
<td >Path</td>
<td >Y</td>
<td >订阅id</td>
<td >4457971f-66c5-4743-8e14-e24627609aff</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····tag</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称</td>
</tr>
<tr>
<td >····equipName</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称</td>
</tr>
<tr>
<td >····val</td>
<td colspan="2">number</td>
<td colspan="3" >点位最后更新数值</td>
</tr>
<tr>
<td >····ts</td>
<td colspan="2">integer</td>
<td colspan="3" >点位最后更新时间</td>
</tr>
<tr>
<td >····quality</td>
<td colspan="2">string</td>
<td colspan="3" >点位数据质量通用实体</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"tag": "PLSDCluster.pm8000\\LPHD1\\EEHealth.on",
········"equipName": "PLSDCluster.pm8000",
········"val": 1.0,
········"ts": 1656897191977,
········"quality": "Good|Bad|Uncertain"
····}
]

</td>
</tr></table>

### 点位时间间隔订阅
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >RequestTagSubscriptionByInterval</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >通过实时数据订阅接口可从PO系统中根据订阅点位和订阅策略组成的订阅条件获得实时点位数据。订阅策略分为点位值变化触发和定时触发两种。其中点位值变化触发策略允许通过变化阈值的类型和数值实现对死区的模拟；定时触发订阅则忽略点位值的变化与否，仅通过预定义的时间间隔推送最新的点位值。 <br>
注意：<br>
1.对于点位值变化触发策略来说，只要点位值的质量（Good/Bad）发生变化，则必然会推送该点位的最新值与质量，无论该点位值是否超过了订阅中对该点位的死区定义。<br>
2.由于点位值的更新本质上是基于轮询机制而存在客观存在的系统采样周期，如果真实世界中的物理值的变化频率高于系统既有的采样频率的两倍，则可能导致点位值变化订阅推送未能反映应有的点位变化信息而丢失关键数据（详情可查阅奈奎斯特-香农采样定律）。例如：PO与下位系统之间通过Modbus协议进行通讯，虽然轮询周期为50毫秒（忽略模数转换的时间与发起策略），但由于通过一个北向接口实例订阅的点位数量为50万点，该北向接口完成一次50万点轮询的时间为60秒，在此种采样周期下（50毫秒被60秒吸收为60秒），一个以60秒为周期的正弦信号由于欠采样，会被误读为恒定值而丧失关键信息。因此对于快变量的采样，需要单独安排独立的PO客户端-北向接口组合以保证其采样频率处于可接受的范围内。</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >header中access_token</td>
<td >eyJhbGciO...iJIUzI1NiIs</td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····tag</td>
<td >string</td>
<td >Body</td>
<td ></td>
<td colspan="2">需要订阅的点位名称</td>
</tr>
<tr>
<td >····type</td>
<td >string</td>
<td >Body</td>
<td ></td>
<td colspan="2">点位值变化阈值类型：
ABS：变化绝对值
PCT：变化百分比
默认值：ABS</td>
</tr>
<tr>
<td >····upper</td>
<td >number</td>
<td >Body</td>
<td ></td>
<td colspan="2">上阈值
默认值：0</td>
</tr>
<tr>
<td >····lower</td>
<td >number</td>
<td >Body</td>
<td ></td>
<td colspan="2">下阈值
默认值：与上阈值相同</td>
</tr>
<tr>
<td >····interval</td>
<td >integer</td>
<td >Body</td>
<td ></td>
<td colspan="2">点位值订阅时间间隔，单位：秒
默认值：900（15分钟）</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

[
····{
········"tag": "c1.D3_F1P01_TH_1\\PLC_Roomhumidity\\HALM",
········"type": "ABS",
········"upper": 0.0,
········"lower": 0.0,
········"interval": 10
····}
]

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >id</td>
<td colspan="2">string</td>
<td colspan="3" >The Subscription Id</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

{
····"id": "string"
}

</td>
</tr></table>

### 点位时间间隔推送
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >ConnectIntervalTagSubscriptionAsync</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >请求示例：
ws://pso-api-server.com/subscriptions/tags/values/latest-by-interval/1e455915-e4fc-419a-8cd8-2cbf6c06b0d9
当WebSocket连接建立后，北向接口将会推送订阅中的所有点位的最新已知数据。
当既定订阅策略中的条件被满足时，北向接口将会通过已经建立好的WebSocket连接推送点位数据。 <br>
 注意： <br>
1.基于RtDataIntervalTimer定时任务，查询间隔见可由参数指定TagSubscriptionRequest.Interval默认配置900s即15min; <br>
2.当缓存开启从缓存查询，当缓存关闭从CtApi查询；<br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >id</td>
<td >string</td>
<td >Path</td>
<td >Y</td>
<td >订阅id</td>
<td >4457971f-66c5-4743-8e14-e24627609aff</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····tag</td>
<td colspan="2">string</td>
<td colspan="3" >点位名称</td>
</tr>
<tr>
<td >····equipName</td>
<td colspan="2">string</td>
<td colspan="3" >设备名称</td>
</tr>
<tr>
<td >····val</td>
<td colspan="2">number</td>
<td colspan="3" >点位最后更新数值</td>
</tr>
<tr>
<td >····ts</td>
<td colspan="2">integer</td>
<td colspan="3" >点位最后更新时间</td>
</tr>
<tr>
<td >····quality</td>
<td colspan="2">string</td>
<td colspan="3" >点位数据质量通用实体</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"tag": "PLSDCluster.pm8000\\LPHD1\\EEHealth.on",
········"equipName": "PLSDCluster.pm8000",
········"val": 1.0,
········"ts": 1656897191977,
········"quality": "Good|Bad|Uncertain"
····}
]

</td>
</tr></table>

### 告警触发订阅
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >RequestEventSubscription</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >返回告警订阅ID
通过实时数据订阅接口可从PO系统中根据订阅由指定点位/层级节点触发的报警/事件。</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >header中access_token</td>
<td >4457971f-66c5-4743-8e14-e24627609aff</td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····tags</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">点位集合</td>
</tr>
<tr>
<td >····equips</td>
<td >array</td>
<td >Body</td>
<td ></td>
<td colspan="2">设备集合</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"tags": "string",
····"equips": [
········"c3.D3_F3IT01_WTH_1"
····]
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >id</td>
<td colspan="2">string</td>
<td colspan="3" >The Subscription Id</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

{
····"id": "string"
}

</td>
</tr></table>

### 告警触发推送
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >EstablishConnectionForEventSubscriptionAsync</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >请求示例：
ws://pso-api-server.com/subscriptions/events/1e455915-e4fc-419a-8cd8-2cbf6c06b0d9
接收告警订阅ID所指定点位的告警信息</td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >id</td>
<td >string</td>
<td >Path</td>
<td >Y</td>
<td >The subscription id.</td>
<td ></td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >text/plain</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >····tag</td>
<td colspan="2">string</td>
<td colspan="3" >报警点位</td>
</tr>
<tr>
<td >····category</td>
<td colspan="2">string</td>
<td colspan="3" >类别</td>
</tr>
<tr>
<td >····equip</td>
<td colspan="2">string</td>
<td colspan="3" >设备</td>
</tr>
<tr>
<td >····msg</td>
<td colspan="2">string</td>
<td colspan="3" >消息</td>
</tr>
<tr>
<td >····desc</td>
<td colspan="2">string</td>
<td colspan="3" >报警点位描述</td>
</tr>
<tr>
<td >····state</td>
<td colspan="2">string</td>
<td colspan="3" >状态</td>
</tr>
<tr>
<td >····ts</td>
<td colspan="2">integer</td>
<td colspan="3" >报警发生事件</td>
</tr>
<tr>
<td >····comment</td>
<td colspan="2">string</td>
<td colspan="3" >备注</td>
</tr>
<tr>
<td >····type</td>
<td colspan="2">string</td>
<td colspan="3" >报警类型</td>
</tr>
<tr>
<td >····alarmState</td>
<td colspan="2">string</td>
<td colspan="3" >报警状态</td>
</tr>
<tr>
<td >····value</td>
<td colspan="2">integer</td>
<td colspan="3" >告警值</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

[
····{
········"tag": "c3.D3_F3IT01_WTH_1\\PLC_H8010N2131_Temperature",
········"category": "1002",
········"equip": "c3.D3F3.IT01.Environment.TemperatureAndHumidity.D3_F3IT01_WTH_1",
········"msg": "WTH1室外温度",
········"desc": "WTH1室外温度",
········"state": "Active|Normal|Acknowledged|Reset",
········"ts": 1658731114000,
········"comment": "string",
········"type": "Analog",
········"alarmState": "Expired",
········"value": 0
····}
]

</td>
</tr></table> 
## 5、点位控制功能接口 


### 控制点位数值
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >TagControlAction</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  >注意：<br> 
1.通过点值写入接口，可对PO系统中的控制点位的值进行控制写入。用于控制的点位必须是可控制点（遥调、遥控）。控制点值写入接口请求路径为：  /tags/{tag }/values/set-point <br> 
2.请注意其中的点位名称必须进行必要的URL编码以免因为点位名称中所含有的特殊字符导致URL歧义而无法被北向接口所识别。<br></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Post</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >tag</td>
<td >string</td>
<td >Path</td>
<td >Y</td>
<td >Tag</td>
<td >c3.D3_F3IT01_WTH_1\PLC_H8010N2131_Temperature</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td >token</td>
<td >4457971f-66c5-4743-8e14-e24627609aff</td>
</tr><tr>
<td >privilege</td>
<td >integer</td>
<td >Query</td>
<td >N</td>
<td >权限组</td>
<td >3</td>
</tr>
<tr>
<td bgcolor="#ddd">参数名</td>
<td bgcolor="#ddd">数据类型</td>
<td bgcolor="#ddd">参数类型</td>
<td bgcolor="#ddd">必需</td>
<td colspan="2" bgcolor="#ddd">描述</td>
</tr>
<tr>
<td >····val</td>
<td >number</td>
<td >Body</td>
<td ></td>
<td colspan="2">修改值</td>
</tr>
<tr>
<td colspan="6"  >示例</td>
</tr>
<tr>
<tr>
<td colspan="6"  >

{
····"val": 1.0
}

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">204</td>
<td colspan="2">No Content</td>
<td colspan="2" ></td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >



</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">400</td>
<td colspan="2">Bad Request</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">boolean</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

false

</td>
</tr></table> 
## 6、配置功能接口 


### 
\r\n<table>
<tr bgcolor="#ddd">
<td >OperationId</td>
<td colspan="5" >GetCtApiSetting</td>
</tr>

<tr>
<td >接口描述</td>
<td colspan="5"  ></td>
</tr>
<tr>
<td >请求方式</td>
<td colspan="5"  >Get</td>
</tr>
<tr>
<td>参数名</td>
<td>数据类型</td>
<td>参数类型</td>
<td>必需</td>
<td>描述</td>
<td>示例</td>
</tr><tr>
<td >access_token</td>
<td >string</td>
<td >Header</td>
<td >N</td>
<td ></td>
<td ></td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">401</td>
<td colspan="2">Unauthorized</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >_</td>
<td colspan="2">string</td>
<td colspan="3" ></td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

"string"

</td>
</tr>
<tr>
<td colspan="2" bgcolor="#ddd">状态码</td>
<td colspan="2" bgcolor="#ddd">描述</td>
<td colspan="2" bgcolor="#ddd">类型</td>
</tr><tr>
<td colspan="2">200</td>
<td colspan="2">Success</td>
<td colspan="2" >application/json</td>
</tr>
<tr>
<td bgcolor="#ddd">返回属性名</td>
<td colspan="2" bgcolor="#ddd">数据类型</td>
<td colspan="3" bgcolor="#ddd">说明</td>
</tr>
<tr>
<td >computer</td>
<td colspan="2">string</td>
<td colspan="3" >PO地址</td>
</tr>
<tr>
<td >userName</td>
<td colspan="2">string</td>
<td colspan="3" >PO管理员账号</td>
</tr>
<tr>
<td >password</td>
<td colspan="2">string</td>
<td colspan="3" >PO管理员密码</td>
</tr>
<tr>
<td >示例</td>
<td colspan="6"  >

{
····"computer": "string",
····"userName": "string",
····"password": "string"
}

</td>
</tr></table>