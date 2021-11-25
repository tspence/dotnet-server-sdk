/* 
 * DevCycle Bucketing API
 *
 * Documents the DevCycle Bucketing API which provides and API interface to User Bucketing and for generated SDKs.
 *
 * OpenAPI spec version: 1.0.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DevCycle.Model
{
    [DataContract]
    public partial class User : IEquatable<User>
    {
        /// <summary>
        /// DevCycle SDK type
        /// </summary>
        /// <value>DevCycle SDK type</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SdkTypeEnum
        {
            /// <summary>
            /// Enum Api for value: api
            /// </summary>
            [EnumMember(Value = "api")]
            Api = 1,
            /// <summary>
            /// Enum Server for value: server
            /// </summary>
            [EnumMember(Value = "server")]
            Server = 2
        }

        /// <summary>
        /// DevCycle SDK type
        /// </summary>
        /// <value>DevCycle SDK type</value>
        [DataMember(Name="sdkType", EmitDefaultValue=false)]
        public SdkTypeEnum? SdkType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="User" /> class.
        /// </summary>
        /// <param name="userId">Unique id to identify the user (required).</param>
        /// <param name="email">User&#x27;s email used to identify the user on the dashboard / target audiences.</param>
        /// <param name="name">User&#x27;s name used to identify the user on the dashboard / target audiences.</param>
        /// <param name="language">User&#x27;s language in ISO 639-1 format.</param>
        /// <param name="country">User&#x27;s country in ISO 3166 alpha-2 format.</param>
        /// <param name="appVersion">App Version of the running application.</param>
        /// <param name="appBuild">App Build number of the running application.</param>
        /// <param name="customData">User&#x27;s custom data to target the user with, data will be logged to DevCycle for use in dashboard..</param>
        /// <param name="privateCustomData">User&#x27;s custom data to target the user with, data will not be logged to DevCycle only used for feature bucketing..</param>
        /// <param name="createdDate">Date the user was created, Unix epoch timestamp format.</param>
        /// <param name="lastSeenDate">Date the user was created, Unix epoch timestamp format.</param>
        /// <param name="platform">Platform the Client SDK is running on.</param>
        /// <param name="platformVersion">Version of the platform the Client SDK is running on.</param>
        /// <param name="deviceModel">User&#x27;s device model.</param>
        /// <param name="sdkType">DevCycle SDK type.</param>
        /// <param name="sdkVersion">DevCycle SDK Version.</param>
        public User(string userId = default, string email = default, string name = default, string language = default, string country = default,
            string appVersion = default, string appBuild = default, Object customData = default, Object privateCustomData = default,
            long? createdDate = default, long? lastSeenDate = default, string platform = default, string platformVersion = default,
            string deviceModel = default, SdkTypeEnum? sdkType = default, string sdkVersion = default)
        {
            // to ensure "userId" is required (not null)
            if (userId == null)
            {
                throw new InvalidDataException("userId is a required property for User and cannot be null");
            }
            else
            {
                this.UserId = userId;
            }
            this.Email = email;
            this.Name = name;
            this.Language = language;
            this.Country = country;
            this.AppVersion = appVersion;
            this.AppBuild = appBuild;
            this.CustomData = customData;
            this.PrivateCustomData = privateCustomData;
            this.CreatedDate = createdDate;
            this.LastSeenDate = lastSeenDate;
            this.Platform = platform;
            this.PlatformVersion = platformVersion;
            this.DeviceModel = deviceModel;
            this.SdkType = sdkType;
            this.SdkVersion = sdkVersion;
        }
        
        /// <summary>
        /// Unique id to identify the user
        /// </summary>
        /// <value>Unique id to identify the user</value>
        [DataMember(Name="user_id", EmitDefaultValue=false)]
        public string UserId { get; set; }

        /// <summary>
        /// User&#x27;s email used to identify the user on the dashboard / target audiences
        /// </summary>
        /// <value>User&#x27;s email used to identify the user on the dashboard / target audiences</value>
        [DataMember(Name="email", EmitDefaultValue=false)]
        public string Email { get; set; }

        /// <summary>
        /// User&#x27;s name used to identify the user on the dashboard / target audiences
        /// </summary>
        /// <value>User&#x27;s name used to identify the user on the dashboard / target audiences</value>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// User&#x27;s language in ISO 639-1 format
        /// </summary>
        /// <value>User&#x27;s language in ISO 639-1 format</value>
        [DataMember(Name="language", EmitDefaultValue=false)]
        public string Language { get; set; }

        /// <summary>
        /// User&#x27;s country in ISO 3166 alpha-2 format
        /// </summary>
        /// <value>User&#x27;s country in ISO 3166 alpha-2 format</value>
        [DataMember(Name="country", EmitDefaultValue=false)]
        public string Country { get; set; }

        /// <summary>
        /// App Version of the running application
        /// </summary>
        /// <value>App Version of the running application</value>
        [DataMember(Name="appVersion", EmitDefaultValue=false)]
        public string AppVersion { get; set; }

        /// <summary>
        /// App Build number of the running application
        /// </summary>
        /// <value>App Build number of the running application</value>
        [DataMember(Name="appBuild", EmitDefaultValue=false)]
        public string AppBuild { get; set; }

        /// <summary>
        /// User&#x27;s custom data to target the user with, data will be logged to DevCycle for use in dashboard.
        /// </summary>
        /// <value>User&#x27;s custom data to target the user with, data will be logged to DevCycle for use in dashboard.</value>
        [DataMember(Name="customData", EmitDefaultValue=false)]
        public Object CustomData { get; set; }

        /// <summary>
        /// User&#x27;s custom data to target the user with, data will not be logged to DevCycle only used for feature bucketing.
        /// </summary>
        /// <value>User&#x27;s custom data to target the user with, data will not be logged to DevCycle only used for feature bucketing.</value>
        [DataMember(Name="privateCustomData", EmitDefaultValue=false)]
        public Object PrivateCustomData { get; set; }

        /// <summary>
        /// Date the user was created, Unix epoch timestamp format
        /// </summary>
        /// <value>Date the user was created, Unix epoch timestamp format</value>
        [DataMember(Name="createdDate", EmitDefaultValue=false)]
        public long? CreatedDate { get; set; }

        /// <summary>
        /// Date the user was created, Unix epoch timestamp format
        /// </summary>
        /// <value>Date the user was created, Unix epoch timestamp format</value>
        [DataMember(Name="lastSeenDate", EmitDefaultValue=false)]
        public long? LastSeenDate { get; set; }

        /// <summary>
        /// Platform the Client SDK is running on
        /// </summary>
        /// <value>Platform the Client SDK is running on</value>
        [DataMember(Name="platform", EmitDefaultValue=false)]
        public string Platform { get; set; }

        /// <summary>
        /// Version of the platform the Client SDK is running on
        /// </summary>
        /// <value>Version of the platform the Client SDK is running on</value>
        [DataMember(Name="platformVersion", EmitDefaultValue=false)]
        public string PlatformVersion { get; set; }

        /// <summary>
        /// User&#x27;s device model
        /// </summary>
        /// <value>User&#x27;s device model</value>
        [DataMember(Name="deviceModel", EmitDefaultValue=false)]
        public string DeviceModel { get; set; }


        /// <summary>
        /// DevCycle SDK Version
        /// </summary>
        /// <value>DevCycle SDK Version</value>
        [DataMember(Name="sdkVersion", EmitDefaultValue=false)]
        public string SdkVersion { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class UserData {\n");
            sb.Append("  UserId: ").Append(UserId).Append("\n");
            sb.Append("  Email: ").Append(Email).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Language: ").Append(Language).Append("\n");
            sb.Append("  Country: ").Append(Country).Append("\n");
            sb.Append("  AppVersion: ").Append(AppVersion).Append("\n");
            sb.Append("  AppBuild: ").Append(AppBuild).Append("\n");
            sb.Append("  CustomData: ").Append(CustomData).Append("\n");
            sb.Append("  PrivateCustomData: ").Append(PrivateCustomData).Append("\n");
            sb.Append("  CreatedDate: ").Append(CreatedDate).Append("\n");
            sb.Append("  LastSeenDate: ").Append(LastSeenDate).Append("\n");
            sb.Append("  Platform: ").Append(Platform).Append("\n");
            sb.Append("  PlatformVersion: ").Append(PlatformVersion).Append("\n");
            sb.Append("  DeviceModel: ").Append(DeviceModel).Append("\n");
            sb.Append("  SdkType: ").Append(SdkType).Append("\n");
            sb.Append("  SdkVersion: ").Append(SdkVersion).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as User);
        }

        /// <summary>
        /// Returns true if UserData instances are equal
        /// </summary>
        /// <param name="input">Instance of UserData to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(User input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.UserId == input.UserId ||
                    (this.UserId != null &&
                    this.UserId.Equals(input.UserId))
                ) && 
                (
                    this.Email == input.Email ||
                    (this.Email != null &&
                    this.Email.Equals(input.Email))
                ) && 
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.Language == input.Language ||
                    (this.Language != null &&
                    this.Language.Equals(input.Language))
                ) && 
                (
                    this.Country == input.Country ||
                    (this.Country != null &&
                    this.Country.Equals(input.Country))
                ) && 
                (
                    this.AppVersion == input.AppVersion ||
                    (this.AppVersion != null &&
                    this.AppVersion.Equals(input.AppVersion))
                ) && 
                (
                    this.AppBuild == input.AppBuild ||
                    (this.AppBuild != null &&
                    this.AppBuild.Equals(input.AppBuild))
                ) && 
                (
                    this.CustomData == input.CustomData ||
                    (this.CustomData != null &&
                    this.CustomData.Equals(input.CustomData))
                ) && 
                (
                    this.PrivateCustomData == input.PrivateCustomData ||
                    (this.PrivateCustomData != null &&
                    this.PrivateCustomData.Equals(input.PrivateCustomData))
                ) && 
                (
                    this.CreatedDate == input.CreatedDate ||
                    (this.CreatedDate != null &&
                    this.CreatedDate.Equals(input.CreatedDate))
                ) && 
                (
                    this.LastSeenDate == input.LastSeenDate ||
                    (this.LastSeenDate != null &&
                    this.LastSeenDate.Equals(input.LastSeenDate))
                ) && 
                (
                    this.Platform == input.Platform ||
                    (this.Platform != null &&
                    this.Platform.Equals(input.Platform))
                ) && 
                (
                    this.PlatformVersion == input.PlatformVersion ||
                    (this.PlatformVersion != null &&
                    this.PlatformVersion.Equals(input.PlatformVersion))
                ) && 
                (
                    this.DeviceModel == input.DeviceModel ||
                    (this.DeviceModel != null &&
                    this.DeviceModel.Equals(input.DeviceModel))
                ) && 
                (
                    this.SdkType == input.SdkType ||
                    (this.SdkType != null &&
                    this.SdkType.Equals(input.SdkType))
                ) && 
                (
                    this.SdkVersion == input.SdkVersion ||
                    (this.SdkVersion != null &&
                    this.SdkVersion.Equals(input.SdkVersion))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.UserId != null)
                    hashCode = hashCode * 59 + this.UserId.GetHashCode();
                if (this.Email != null)
                    hashCode = hashCode * 59 + this.Email.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Language != null)
                    hashCode = hashCode * 59 + this.Language.GetHashCode();
                if (this.Country != null)
                    hashCode = hashCode * 59 + this.Country.GetHashCode();
                if (this.AppVersion != null)
                    hashCode = hashCode * 59 + this.AppVersion.GetHashCode();
                if (this.AppBuild != null)
                    hashCode = hashCode * 59 + this.AppBuild.GetHashCode();
                if (this.CustomData != null)
                    hashCode = hashCode * 59 + this.CustomData.GetHashCode();
                if (this.PrivateCustomData != null)
                    hashCode = hashCode * 59 + this.PrivateCustomData.GetHashCode();
                if (this.CreatedDate != null)
                    hashCode = hashCode * 59 + this.CreatedDate.GetHashCode();
                if (this.LastSeenDate != null)
                    hashCode = hashCode * 59 + this.LastSeenDate.GetHashCode();
                if (this.Platform != null)
                    hashCode = hashCode * 59 + this.Platform.GetHashCode();
                if (this.PlatformVersion != null)
                    hashCode = hashCode * 59 + this.PlatformVersion.GetHashCode();
                if (this.DeviceModel != null)
                    hashCode = hashCode * 59 + this.DeviceModel.GetHashCode();
                if (this.SdkType != null)
                    hashCode = hashCode * 59 + this.SdkType.GetHashCode();
                if (this.SdkVersion != null)
                    hashCode = hashCode * 59 + this.SdkVersion.GetHashCode();
                return hashCode;
            }
        }
    }
}
