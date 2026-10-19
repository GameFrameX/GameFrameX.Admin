// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯国家机关、社会团体和其他组织以及个人的合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using System.Text.Json;
using GameFrameX.Core.Extension.Json;
using GameFrameX.Foundation.Json;

namespace GameFrameX.Application.Game;

/// <summary>
/// 内嵌 JSON 列（AttachmentsJson / Contents / ItemsJson 等 string 列内嵌对象集合）的序列化选项。
/// <para>
/// C270：HTTP 层的 long→string 契约只作用于响应属性级 long，不覆盖 string 属性内嵌的 JSON 内容；
/// 内嵌集合里的雪花 Id（如 <c>GamePropItem.Id</c>）经 <see cref="JsonHelper.Serialize(object)"/>
/// 会以裸 number 落库，前端 <c>JSON.parse</c> 即超 2^53 丢精度。此选项在 <see cref="JsonHelper.DefaultOptions"/>
/// 拷贝基础上挂 <see cref="ProjectJsonSerializerOptions.ApplyLongIdentifierStringContract"/>，
/// 写方向 *Id 输出字符串；读方向 JsonHelper 自带 AllowReadingFromString，新旧形态均可读。
/// </para>
/// </summary>
public static class EmbeddedJsonOptions
{
    public static readonly JsonSerializerOptions Instance = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonHelper.DefaultOptions);
        ProjectJsonSerializerOptions.ApplyLongIdentifierStringContract(options);
        return options;
    }
}
