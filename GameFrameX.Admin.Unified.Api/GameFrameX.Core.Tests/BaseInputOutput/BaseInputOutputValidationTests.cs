using System.ComponentModel.DataAnnotations;
using System.Reflection;
using GameFrameX.Application.System.Dictionary.Dto;
using GameFrameX.Wechat.Open.Dto;
using GameFrameX.Core.BaseInputOutput;

namespace GameFrameX.Core.Tests.BaseInputOutput;

public class BaseInputOutputValidationTests
{
    [Fact]
    public void BasePageInput_DefaultValues_AreValidAndStable()
    {
        var input = new BasePageRequest();

        var results = Validate(input);

        Assert.Empty(results);
        Assert.Equal(1, input.Page);
        Assert.Equal(20, input.PageSize);
        Assert.Equal(string.Empty, input.OrderBy);
        Assert.False(input.IsDescending);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BasePageInput_PageLessThanOne_IsInvalid(int page)
    {
        var input = new BasePageRequest { Page = page, };

        var results = Validate(input);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(BasePageRequest.Page)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void BasePageInput_PageSizeOutsideAllowedRange_IsInvalid(int pageSize)
    {
        var input = new BasePageRequest { PageSize = pageSize, };

        var results = Validate(input);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(BasePageRequest.PageSize)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void BasePageInput_PageSizeInsideAllowedRange_IsValid(int pageSize)
    {
        var input = new BasePageRequest { PageSize = pageSize, };

        var results = Validate(input);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BaseIdInput_IdLessThanOne_IsInvalid(long id)
    {
        var input = new BaseIdRequest { Id = id, };

        var results = Validate(input);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(BaseIdRequest.Id)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BaseUpdateInput_IdLessThanOne_IsInvalid(long id)
    {
        var input = new BaseUpdateRequest { Id = id, };

        var results = Validate(input);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(BaseUpdateRequest.Id)));
    }

    [Fact]
    public void IsDefaultNamedAddInput_NameMissing_IsInvalid()
    {
        var input = new BaseIsDefaultNamedAddRequest();

        var results = Validate(input);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(BaseIsDefaultNamedAddRequest.Name)));
    }

    [Fact]
    public void AppFilterPageInput_InheritsPageValidation()
    {
        var input = new BaseAppFilterPageRequest { Page = 0, PageSize = 101, };

        var results = Validate(input);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(BaseAppFilterPageRequest.Page)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(BaseAppFilterPageRequest.PageSize)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetDataDictionaryDataInput_DictionaryTypeIdLessThanOne_IsInvalid(long dictionaryTypeId)
    {
        var input = new GetDataDictionaryDataRequest { DictionaryTypeId = dictionaryTypeId, };

        var results = Validate(input);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(GetDataDictionaryDataRequest.DictionaryTypeId)));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcdefghijk")]
    public void WxPhoneLoginInput_InvalidPhoneNumber_IsInvalid(string phoneNumber)
    {
        var input = new WxPhoneLoginRequest { PhoneNumber = phoneNumber, };

        var results = Validate(input);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(WxPhoneLoginRequest.PhoneNumber)));
    }

    [Fact]
    public void WxPhoneLoginInput_ChinaMobileNumber_IsValid()
    {
        var input = new WxPhoneLoginRequest { PhoneNumber = "13800138000", };

        var results = Validate(input);

        Assert.Empty(results);
    }

    [Fact]
    public void BaseInputs_DoNotUse历史框架DataValidationAttribute()
    {
        var properties = new[]
        {
            typeof(BaseIdRequest).GetProperty(nameof(BaseIdRequest.Id)),
            typeof(BasePageRequest).GetProperty(nameof(BasePageRequest.Page)),
            typeof(BasePageRequest).GetProperty(nameof(BasePageRequest.PageSize)),
            typeof(GetDataDictionaryDataRequest).GetProperty(nameof(GetDataDictionaryDataRequest.DictionaryTypeId)),
            typeof(WxPhoneLoginRequest).GetProperty(nameof(WxPhoneLoginRequest.PhoneNumber)),
        };

        foreach (var property in properties)
        {
            var attributeTypeNames = property!
                                     .GetCustomAttributes(true)
                                     .Select(u => u.GetType().FullName);

            Assert.DoesNotContain("历史框架.DataValidation.DataValidationAttribute", attributeTypeNames);
        }
    }

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        foreach (var property in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var value = property.GetValue(instance);
            var context = new ValidationContext(instance)
            {
                MemberName = property.Name,
            };

            foreach (var attribute in GetStandardValidationAttributes(property))
            {
                var result = attribute.GetValidationResult(value, context);
                if (result != ValidationResult.Success)
                {
                    results.Add(result!);
                }
            }
        }

        return results;
    }

    private static IEnumerable<ValidationAttribute> GetStandardValidationAttributes(PropertyInfo property)
    {
        return property
               .GetCustomAttributes<ValidationAttribute>(true)
               .Where(attribute => attribute is RequiredAttribute or RangeAttribute or RegularExpressionAttribute);
    }
}