s/if (status_ == 200)/if (status_ >= 200 \&\& status_ < 300)/g
s/Required = Newtonsoft.Json.Required.DisallowNull/Required = Newtonsoft.Json.Required.Default/g
