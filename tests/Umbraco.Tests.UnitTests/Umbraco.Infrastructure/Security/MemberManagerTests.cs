using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

[TestFixture]
public class MemberManagerTests
{
    private MemberUserStore _fakeMemberStore;
    private Mock<IOptions<IdentityOptions>> _mockIdentityOptions;
    private Mock<IPasswordHasher<MemberIdentityUser>> _mockPasswordHasher;
    private Mock<IMemberService> _mockMemberService;
    private Mock<IServiceProvider> _mockServiceProviders;
    private Mock<IOptionsSnapshot<MemberPasswordConfigurationSettings>> _mockPasswordConfiguration;

    public MemberManager CreateSut()
    {
        var scopeProvider = TestHelper.ScopeProvider;
        _mockMemberService = new Mock<IMemberService>();

        var mapDefinitions = new List<IMapDefinition>
        {
            new IdentityMapDefinition(
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IEntityService>(),
                new TestOptionsSnapshot<GlobalSettings>(new GlobalSettings()),
                new TestOptionsSnapshot<SecuritySettings>(new SecuritySettings()),
                AppCaches.Disabled,
                Mock.Of<ITwoFactorLoginService>())
        };

        _fakeMemberStore = new MemberUserStore(
            _mockMemberService.Object,
            new UmbracoMapper(new MapDefinitionCollection(() => mapDefinitions), scopeProvider, NullLogger<UmbracoMapper>.Instance),
            scopeProvider,
            new IdentityErrorDescriber(),
            Mock.Of<IExternalLoginWithKeyService>(),
            Mock.Of<ITwoFactorLoginService>(),
            Mock.Of<IPublishedMemberCache>());

        _mockIdentityOptions = new Mock<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions { Lockout = { AllowedForNewUsers = false } };
        _mockIdentityOptions.Setup(o => o.Value).Returns(idOptions);
        _mockPasswordHasher = new Mock<IPasswordHasher<MemberIdentityUser>>();

        var userValidators = new List<IUserValidator<MemberIdentityUser>>();
        var validator = new Mock<IUserValidator<MemberIdentityUser>>();
        userValidators.Add(validator.Object);

        _mockServiceProviders = new Mock<IServiceProvider>();
        _mockPasswordConfiguration = new Mock<IOptionsSnapshot<MemberPasswordConfigurationSettings>>();
        _mockPasswordConfiguration.Setup(x => x.Value).Returns(() =>
            new MemberPasswordConfigurationSettings());

        var pwdValidators = new List<PasswordValidator<MemberIdentityUser>> { new() };

        var userManager = new MemberManager(
            new Mock<IIpResolver>().Object,
            _fakeMemberStore,
            _mockIdentityOptions.Object,
            _mockPasswordHasher.Object,
            userValidators,
            pwdValidators,
            new MembersErrorDescriber(Mock.Of<ILocalizedTextService>()),
            _mockServiceProviders.Object,
            new Mock<ILogger<UserManager<MemberIdentityUser>>>().Object,
            _mockPasswordConfiguration.Object,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IHttpContextAccessor>());

        validator.Setup(v => v.ValidateAsync(
                userManager,
                It.IsAny<MemberIdentityUser>()))
            .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

        return userManager;
    }

    [Test]
    public async Task GivenICreateUser_AndTheIdentityResultFailed_ThenIShouldGetAFailedResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser { PasswordConfig = "testConfig" };

        // act
        var identityResult = await sut.CreateAsync(fakeUser);

        // assert
        Assert.IsFalse(identityResult.Succeeded);
        Assert.IsFalse(!identityResult.Errors.Any());
    }

    [Test]
    public async Task GivenICreateUser_AndTheUserIsNull_ThenIShouldGetAFailedResultAsync()
    {
        // arrange
        var sut = CreateSut();
        IdentityError[] identityErrors =
        {
            new() { Code = "IdentityError1", Description = "There was an identity error when creating a user" },
        };

        // act
        Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.CreateAsync(null));
    }

    [Test]
    public async Task GivenICreateANewUser_AndTheUserIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeUser = CreateValidUser();

        var fakeMember = CreateMember(fakeUser);

        MockMemberServiceForCreateMember(fakeMember);

        // act
        var identityResult = await sut.CreateAsync(fakeUser);

        // assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsFalse(identityResult.Errors.Any());
    }

    [Test]
    public async Task GivenAApprovedUserExists_AndTheCorrectCredentialsAreProvided_ThenACheckOfCredentialsShouldSucceed()
    {
        // arrange
        var password = "password";
        var sut = CreateSut();

        var fakeUser = CreateValidUser();

        var fakeMember = CreateMember(fakeUser);

        MockMemberServiceForCreateMember(fakeMember);

        _mockMemberService.Setup(x => x.GetByUsername(It.Is<string>(y => y == fakeUser.UserName))).Returns(fakeMember);

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(It.IsAny<MemberIdentityUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Success);

        // act
        await sut.CreateAsync(fakeUser);
        var result = await sut.ValidateCredentialsAsync(fakeUser.UserName, password);

        // assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task GivenAnUnapprovedUserExists_AndTheCorrectCredentialsAreProvided_ThenACheckOfCredentialsShouldFail()
    {
        // arrange
        var password = "password";
        var sut = CreateSut();

        var fakeUser = CreateValidUser();
        fakeUser.IsApproved = false;

        var fakeMember = CreateMember(fakeUser);

        MockMemberServiceForCreateMember(fakeMember);

        _mockMemberService.Setup(x => x.GetByUsername(It.Is<string>(y => y == fakeUser.UserName))).Returns(fakeMember);

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(It.IsAny<MemberIdentityUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Success);

        // act
        await sut.CreateAsync(fakeUser);
        var result = await sut.ValidateCredentialsAsync(fakeUser.UserName, password);

        // assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task GivenAUserExists_AndIncorrectCredentialsAreProvided_ThenACheckOfCredentialsShouldFail()
    {
        // arrange
        var password = "password";
        var sut = CreateSut();

        var fakeUser = CreateValidUser();

        var fakeMember = CreateMember(fakeUser);

        MockMemberServiceForCreateMember(fakeMember);

        _mockMemberService.Setup(x => x.GetByUsername(It.Is<string>(y => y == fakeUser.UserName))).Returns(fakeMember);

        _mockPasswordHasher
            .Setup(x => x.VerifyHashedPassword(It.IsAny<MemberIdentityUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Failed);

        // act
        await sut.CreateAsync(fakeUser);
        var result = await sut.ValidateCredentialsAsync(fakeUser.UserName, password);

        // assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task GivenAUserDoesExists_AndCredentialsAreProvided_ThenACheckOfCredentialsShouldFail()
    {
        // arrange
        var password = "password";
        var sut = CreateSut();

        _mockMemberService.Setup(x => x.GetByUsername(It.Is<string>(y => y == "testUser"))).Returns((IMember)null);

        // act
        var result = await sut.ValidateCredentialsAsync("testUser", password);

        // assert
        Assert.IsFalse(result);
    }

    private static MemberIdentityUser CreateValidUser() =>
        new(777)
        {
            UserName = "testUser",
            Email = "test@test.com",
            Name = "Test",
            MemberTypeAlias = "Anything",
            PasswordConfig = "testConfig",
            PasswordHash = "hashedPassword",
            IsApproved = true
        };

    private static IMember CreateMember(MemberIdentityUser fakeUser)
    {
        var builder = new MemberTypeBuilder();
        var memberType = builder.BuildSimpleMemberType();
        return new Member(memberType) { Id = 777, Username = fakeUser.UserName };
    }

    private void MockMemberServiceForCreateMember(IMember fakeMember)
    {
        _mockMemberService
            .Setup(x => x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(fakeMember);
        _mockMemberService
            .Setup(x => x.Save(fakeMember, It.IsAny<PublishNotificationSaveOptions>(), Constants.Security.SuperUserId))
            .Returns(Attempt.Succeed<OperationResult?>(null));

    }
}
