# OpenAPI2MD.CommunityToolkit.Example(1.0.0) 
## HelloWorld 
## SayHello 
<table>
    
<tr>
    <td colspan="6" bgcolor="#ddd">SayHello</td>
</tr>
<tr>
    <td >接口名称</td>
    <td colspan="5">SayHello</td>
</tr>
<tr>
    <td >接口描述</td>
    <td colspan="5"></td>
</tr>
<tr>
    <td >URL</td>
    <td colspan="5">/HelloWorld/SayHello</td>
</tr>
<tr>
    <td >请求方式</td>
    <td colspan="5">Get</td>
</tr>
<tr>
    <td >请求类型</td>
    <td colspan="5"></td>
</tr>
    

    
 <tr>
    <td bgcolor="#ddd">状态码</td>
    <td colspan="1" bgcolor="#ddd">描述</td>
    <td colspan="2" bgcolor="#ddd">类型</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
</tr>
<tr>
    <td >200</td>
    <td >Success</td>
    <td colspan="2" >application/json</td>
    <td colspan="2" >string</td>
</tr>
<tr>
    <td bgcolor="#ddd">返回属性名</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
    <td colspan="3" bgcolor="#ddd">说明</td>
    
</tr><tr>
    <td >_</td>
    <td colspan="2">string</td>
    <td colspan="3" ></td>
   
</tr>


<tr>
    <td colspan="6" bgcolor="#ddd">示例</td>
</tr>
<tr>
<td colspan="6">

string
</td>
</tr>




    
    
</table> 
## WeatherForecast 
## GetToken 
<table>
    
<tr>
    <td colspan="6" bgcolor="#ddd">token</td>
</tr>
<tr>
    <td >接口名称</td>
    <td colspan="5">token</td>
</tr>
<tr>
    <td >接口描述</td>
    <td colspan="5"></td>
</tr>
<tr>
    <td >URL</td>
    <td colspan="5">/WeatherForecast/token</td>
</tr>
<tr>
    <td >请求方式</td>
    <td colspan="5">Get</td>
</tr>
<tr>
    <td >请求类型</td>
    <td colspan="5"></td>
</tr>
    

    
 <tr>
    <td bgcolor="#ddd">状态码</td>
    <td colspan="1" bgcolor="#ddd">描述</td>
    <td colspan="2" bgcolor="#ddd">类型</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
</tr>
<tr>
    <td >200</td>
    <td >Success</td>
    <td colspan="2" >application/json</td>
    <td colspan="2" >string</td>
</tr>
<tr>
    <td bgcolor="#ddd">返回属性名</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
    <td colspan="3" bgcolor="#ddd">说明</td>
    
</tr><tr>
    <td >_</td>
    <td colspan="2">string</td>
    <td colspan="3" ></td>
   
</tr>


<tr>
    <td colspan="6" bgcolor="#ddd">示例</td>
</tr>
<tr>
<td colspan="6">

string
</td>
</tr>




    
    
</table> 
## GetById 
<table>
    
<tr>
    <td colspan="6" bgcolor="#ddd">查天气</td>
</tr>
<tr>
    <td >接口名称</td>
    <td colspan="5">查天气</td>
</tr>
<tr>
    <td >接口描述</td>
    <td colspan="5">这是一个测试接口</td>
</tr>
<tr>
    <td >URL</td>
    <td colspan="5">/WeatherForecast/{id}</td>
</tr>
<tr>
    <td >请求方式</td>
    <td colspan="5">Get</td>
</tr>
<tr>
    <td >请求类型</td>
    <td colspan="5"></td>
</tr>
    <tr>
    <td bgcolor="#ddd">参数名</td>
    <td bgcolor="#ddd">数据类型</td>
    <td bgcolor="#ddd">参数类型</td>
    <td bgcolor="#ddd">是否必填</td>
    <td bgcolor="#ddd">说明</td>
    <td bgcolor="#ddd">示例</td>
</tr><tr>
    <td >Authorization</td>
    <td >string</td>
    <td >Header</td>
    <td >Y</td>
    <td >令牌</td>
    <td >asdfasfeadsf</td>
</tr><tr>
    <td >id</td>
    <td >integer</td>
    <td >Path</td>
    <td >Y</td>
    <td >参数</td>
    <td >888</td>
</tr>

    
 <tr>
    <td bgcolor="#ddd">状态码</td>
    <td colspan="1" bgcolor="#ddd">描述</td>
    <td colspan="2" bgcolor="#ddd">类型</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
</tr>
<tr>
    <td >200</td>
    <td >Success</td>
    <td colspan="2" >application/json</td>
    <td colspan="2" >array:WeatherForecast</td>
</tr>
<tr>
    <td bgcolor="#ddd">返回属性名</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
    <td colspan="3" bgcolor="#ddd">说明</td>
    
</tr><tr>
    <td >WeatherForecast</td>
    <td colspan="2">array:WeatherForecast</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >····remarks</td>
    <td colspan="2">array:Remark</td>
    <td colspan="3" >备注列表</td>
   
</tr><tr>
    <td >········percent</td>
    <td colspan="2">number</td>
    <td colspan="3" >idc</td>
   
</tr><tr>
    <td >········name</td>
    <td colspan="2">string</td>
    <td colspan="3" >标记</td>
   
</tr><tr>
    <td >········ok</td>
    <td colspan="2">boolean</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >····date</td>
    <td colspan="2">string</td>
    <td colspan="3" >日期</td>
   
</tr><tr>
    <td >····temperatureC</td>
    <td colspan="2">integer</td>
    <td colspan="3" >温度</td>
   
</tr><tr>
    <td >····temperatureF</td>
    <td colspan="2">integer</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >····summary</td>
    <td colspan="2">string</td>
    <td colspan="3" >汇总</td>
   
</tr><tr>
    <td >····mark</td>
    <td colspan="2">object:Remark</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >········percent</td>
    <td colspan="2">number</td>
    <td colspan="3" >idc</td>
   
</tr><tr>
    <td >········name</td>
    <td colspan="2">string</td>
    <td colspan="3" >标记</td>
   
</tr><tr>
    <td >········ok</td>
    <td colspan="2">boolean</td>
    <td colspan="3" ></td>
   
</tr>


<tr>
    <td colspan="6" bgcolor="#ddd">示例</td>
</tr>
<tr>
<td colspan="6">

[
····{
········"remarks": [
············{
················"percent": 3.1415932,
················"name": "把大象装冰箱分几步？",
················"ok": true
············}
········],
········"date": "2020-02-02T00:00:00+08:00",
········"temperatureC": 100,
········"temperatureF": 0,
········"summary": "汇总是这样写的吗",
········"mark": {
············"percent": 3.1415932,
············"name": "把大象装冰箱分几步？",
············"ok": true
········}
····}
]
</td>
</tr>



<tr>
    <td bgcolor="#ddd">状态码</td>
    <td colspan="1" bgcolor="#ddd">描述</td>
    <td colspan="2" bgcolor="#ddd">类型</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
</tr>
<tr>
    <td >400</td>
    <td >Bad Request</td>
    <td colspan="2" >application/json</td>
    <td colspan="2" >object:BaseResultModel</td>
</tr>
<tr>
    <td bgcolor="#ddd">返回属性名</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
    <td colspan="3" bgcolor="#ddd">说明</td>
    
</tr><tr>
    <td >code</td>
    <td colspan="2">integer</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >message</td>
    <td colspan="2">string</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >details</td>
    <td colspan="2"></td>
    <td colspan="3" ></td>
   
</tr>


<tr>
    <td colspan="6" bgcolor="#ddd">示例</td>
</tr>
<tr>
<td colspan="6">

{
····"code": 0,
····"message": null,
····"details": null
}
</td>
</tr>



<tr>
    <td bgcolor="#ddd">状态码</td>
    <td colspan="1" bgcolor="#ddd">描述</td>
    <td colspan="2" bgcolor="#ddd">类型</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
</tr>
<tr>
    <td >404</td>
    <td >Not Found</td>
    <td colspan="2" >application/json</td>
    <td colspan="2" >object:BaseResultModel</td>
</tr>
<tr>
    <td bgcolor="#ddd">返回属性名</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
    <td colspan="3" bgcolor="#ddd">说明</td>
    
</tr><tr>
    <td >code</td>
    <td colspan="2">integer</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >message</td>
    <td colspan="2">string</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >details</td>
    <td colspan="2"></td>
    <td colspan="3" ></td>
   
</tr>


<tr>
    <td colspan="6" bgcolor="#ddd">示例</td>
</tr>
<tr>
<td colspan="6">

{
····"code": 0,
····"message": null,
····"details": null
}
</td>
</tr>



<tr>
    <td bgcolor="#ddd">状态码</td>
    <td colspan="1" bgcolor="#ddd">描述</td>
    <td colspan="2" bgcolor="#ddd">类型</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
</tr>
<tr>
    <td >500</td>
    <td >Server Error</td>
    <td colspan="2" >application/json</td>
    <td colspan="2" >object:InternalServerError</td>
</tr>
<tr>
    <td bgcolor="#ddd">返回属性名</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
    <td colspan="3" bgcolor="#ddd">说明</td>
    
</tr><tr>
    <td >message</td>
    <td colspan="2">string</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >exceptionMessage</td>
    <td colspan="2">string</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >exceptionType</td>
    <td colspan="2">string</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >stackTrace</td>
    <td colspan="2">string</td>
    <td colspan="3" ></td>
   
</tr>


<tr>
    <td colspan="6" bgcolor="#ddd">示例</td>
</tr>
<tr>
<td colspan="6">

{
····"message": null,
····"exceptionMessage": null,
····"exceptionType": null,
····"stackTrace": null
}
</td>
</tr>




    
    
</table> 
## Update 
<table>
    
<tr>
    <td colspan="6" bgcolor="#ddd">修改天气记录</td>
</tr>
<tr>
    <td >接口名称</td>
    <td colspan="5">修改天气记录</td>
</tr>
<tr>
    <td >接口描述</td>
    <td colspan="5">这是一个测试接口</td>
</tr>
<tr>
    <td >URL</td>
    <td colspan="5">/WeatherForecast/Update</td>
</tr>
<tr>
    <td >请求方式</td>
    <td colspan="5">Post</td>
</tr>
<tr>
    <td >请求类型</td>
    <td colspan="5"></td>
</tr>
    <tr>
    <td bgcolor="#ddd">参数名</td>
    <td bgcolor="#ddd">数据类型</td>
    <td bgcolor="#ddd">参数类型</td>
    <td bgcolor="#ddd">是否必填</td>
    <td bgcolor="#ddd">说明</td>
    <td bgcolor="#ddd">示例</td>
</tr><tr>
    <td >Authorization</td>
    <td >string</td>
    <td >Header</td>
    <td >Y</td>
    <td >登录i令牌</td>
    <td >3.1415926</td>
</tr>
<tr>
    <td bgcolor="#ddd">参数名</td>
    <td bgcolor="#ddd">数据类型</td>
    <td bgcolor="#ddd">参数类型</td>
    <td bgcolor="#ddd">是否必填</td>
    <td colspan="2" bgcolor="#ddd">说明</td>
</tr><tr>
    <td >WeatherForecast</td>
    <td >object:WeatherForecast</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2"></td>
</tr><tr>
    <td >····remarks</td>
    <td >array</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2">备注列表</td>
</tr><tr>
    <td >········Remark</td>
    <td >array:Remark</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2">备注列表</td>
</tr><tr>
    <td >············percent</td>
    <td >number</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2">idc</td>
</tr><tr>
    <td >············name</td>
    <td >string</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2">标记</td>
</tr><tr>
    <td >············ok</td>
    <td >boolean</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2"></td>
</tr><tr>
    <td >····date</td>
    <td >string</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2">日期</td>
</tr><tr>
    <td >····temperatureC</td>
    <td >integer</td>
    <td >Body</td>
    <td >Y</td>
    <td colspan="2">温度</td>
</tr><tr>
    <td >····temperatureF</td>
    <td >integer</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2"></td>
</tr><tr>
    <td >····summary</td>
    <td >string</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2">汇总</td>
</tr><tr>
    <td >····mark</td>
    <td >object:Remark</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2"></td>
</tr><tr>
    <td >········Remark</td>
    <td >object:Remark</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2"></td>
</tr><tr>
    <td >············percent</td>
    <td >number</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2">idc</td>
</tr><tr>
    <td >············name</td>
    <td >string</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2">标记</td>
</tr><tr>
    <td >············ok</td>
    <td >boolean</td>
    <td >Body</td>
    <td ></td>
    <td colspan="2"></td>
</tr>
    <tr>
    <td colspan="6" bgcolor="#ddd">示例</td>
    </tr>
    <tr>
    <td colspan="6">

 {
····"remarks": [
········{
············"percent": 3.1415932,
············"name": "把大象装冰箱分几步？",
············"ok": true
········}
····],
····"date": "2020-02-02T00:00:00+08:00",
····"temperatureC": 100,
····"temperatureF": 0,
····"summary": "汇总是这样写的吗",
····"mark": {
········"percent": 3.1415932,
········"name": "把大象装冰箱分几步？",
········"ok": true
····}
}</td>
</tr>
 <tr>
    <td bgcolor="#ddd">状态码</td>
    <td colspan="1" bgcolor="#ddd">描述</td>
    <td colspan="2" bgcolor="#ddd">类型</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
</tr>
<tr>
    <td >200</td>
    <td >Success</td>
    <td colspan="2" >application/json</td>
    <td colspan="2" >object:WeatherForecast</td>
</tr>
<tr>
    <td bgcolor="#ddd">返回属性名</td>
    <td colspan="2" bgcolor="#ddd">数据类型</td>
    <td colspan="3" bgcolor="#ddd">说明</td>
    
</tr><tr>
    <td >remarks</td>
    <td colspan="2">array</td>
    <td colspan="3" >备注列表</td>
   
</tr><tr>
    <td >····Remark</td>
    <td colspan="2">array:Remark</td>
    <td colspan="3" >备注列表</td>
   
</tr><tr>
    <td >········percent</td>
    <td colspan="2">number</td>
    <td colspan="3" >idc</td>
   
</tr><tr>
    <td >········name</td>
    <td colspan="2">string</td>
    <td colspan="3" >标记</td>
   
</tr><tr>
    <td >········ok</td>
    <td colspan="2">boolean</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >date</td>
    <td colspan="2">string</td>
    <td colspan="3" >日期</td>
   
</tr><tr>
    <td >temperatureC</td>
    <td colspan="2">integer</td>
    <td colspan="3" >温度</td>
   
</tr><tr>
    <td >temperatureF</td>
    <td colspan="2">integer</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >summary</td>
    <td colspan="2">string</td>
    <td colspan="3" >汇总</td>
   
</tr><tr>
    <td >mark</td>
    <td colspan="2">object:Remark</td>
    <td colspan="3" ></td>
   
</tr><tr>
    <td >····percent</td>
    <td colspan="2">number</td>
    <td colspan="3" >idc</td>
   
</tr><tr>
    <td >····name</td>
    <td colspan="2">string</td>
    <td colspan="3" >标记</td>
   
</tr><tr>
    <td >····ok</td>
    <td colspan="2">boolean</td>
    <td colspan="3" ></td>
   
</tr>


<tr>
    <td colspan="6" bgcolor="#ddd">示例</td>
</tr>
<tr>
<td colspan="6">

{
····"remarks": [
········{
············"percent": 3.1415932,
············"name": "把大象装冰箱分几步？",
············"ok": true
········}
····],
····"date": "2020-02-02T00:00:00+08:00",
····"temperatureC": 100,
····"temperatureF": 0,
····"summary": "汇总是这样写的吗",
····"mark": {
········"percent": 3.1415932,
········"name": "把大象装冰箱分几步？",
········"ok": true
····}
}
</td>
</tr>




    
    
</table> 
