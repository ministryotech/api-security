# Introduction
APIs can be secured in various ways. The Ministry.Web.ApiSecurity package supports 3 distinct forms of API security. Multiple forms of security within a single API can be supported but not, generally, on a single method. This allows internal use and external use applications of APIs in a secure manner.

It is dependent on the [Fluent Guard](../fluent-guard) project.

## NamedStaticApiKey
This is a string key based attribute that can be applied to a controller. As with 'ConfiguredStaticApiKey' it requires **X-ApiKey** to be passed as a header, but in this case the attribute's key relates to the key for the API key value stored in configuration, allowing multiple keys to be used in a single API.

Usage...

    [HttpPost]
    [NamedStaticApiKey("Ministry")]
    public async Task<IActionResult> Post([FromBody] MyModel model)
    {

And in configuration...

    "StaticApiKeys": {
        "Named": [
            {
                "Key": "Ministry",
                "Value": "385k3q84q3k58aq934l2q304294l"
            }
        ]
    }

This suffers from the key downside that anyone intercepting the message will then know the API key to access that endpoint. For this reason, I recommend this option is only used over HTTPS. For more secure API Security, see 'MessageSignatureValidation' or 'HashedApiKey' below.

## MessageSignatureValidation
This attribute provides two arguments, the first is a header name and the second is a key which relates to a secret. The header of the name provided should include a hash value, formatted to include the type. For example...


    X-Signature: sha256=q98w74981n793my373878s3479357al29874s9t2x7al983s798t

...indicates a header called 'X-Signature' which contains a SHA256 hash string which should represent the request body. When applied, the attribute will read the message body and hash it using the secret for the key name provided and check that the hash value generated matches the value provided in the header. If it doesn't match then a 400 result "Signature Validation Failed" is returned.

This is extremely secure as both the API endpoint and the consumer must share a known secret in order to generate comparable hashes of the message body.

Usage...

    [HttpPost]
    [MessageSignatureValidation("X-Signature", "Ministry")]
    public async Task<IActionResult> Post([FromBody] MyModel model)
    {

And in configuration...

    "SignatureSecrets": {
        "Secrets": [
            {
                "Key": "Ministry",
                "Secret": "q309k8503845ak235a435234s02d375h2874"
            },
            {
                "Key": "ThirdParty",
                "Secret": "23904823l84a284028340a2l042a40234a238"
            }
    
        ]
    }

### Testing & Hashing Gotchas
Testing this approach is a little more tricky than the others, as authorisation needs to be provided with a header that varies according to the body. You can generate good test hashes using an online tool such as https://www.freeformatter.com/hmac-generator.html

The hashing methods remove any newline characters from the message body before hashing so it is important that the service consumer does the same thing. This is because newline characters vary in application between OS platforms and may not be comparable. When generating test hashes, make sure to remove any newline characters from the test body in the online tool.

## HashedApiKey
This method combines elements from 'NamedStaticApiKey' and 'MessageSignatureValidation'. It is extremely secure as it uses secrets that must be known by both the API and consumer but it doesn't require hashing method bodies which means that it doesn't suffer from potential newline hashing issues and it can also be used with REST API calls that don't have a body and all request details come through the URL. In this case, the API Key header is passed as a hash. For example...


    X-ApiKey: sha256=q98w74981n793my373878s3479357al29874s9t2x7al983s798t

...indicates a header called 'X-ApiKey' which contains a SHA256 hash string which should represent the request body. When applied, the attribute will read the message body and hash it using the secret for the key name provided and check that the hash value generated matches the value provided in the header. If it doesn't match then a 400 result "Key Validation Failed" is returned.

The downsides to this approach over 'MessageSignatureValidation' is purely that it requires more configuration both for the API and consumer which need to then know both the API Key itself and the secret.

Usage...

    [HttpPost]
    [HashedApiKey("X-ApiKey", "Ministry")]
    public async Task<IActionResult> Post([FromBody] MyModel model)
    {

And in configuration...

    "StaticApiKeys": {
        "Secrets": [
            {
                "Key": "Ministry",
                "Secret": "q309k8503845ak235a435234s02d375h2874",
                "Value": "385k3q84q3k58aq934l2q304294l"
            },
            {
                "Key": "ThirdParty",
                "Secret": "23904823l84a284028340a2l042a40234a238",
                "Value": "4398523s453k427s98s7375v23d5"
            }
    
        ]
    }

You can see from the configuration how you could easily use 'NamedStaticApiKey' and 'HashedApiKey' within the same application. They use the same Options object. In fact, all of these options can easily be combined as needed.

# Consuming Hash Secured APIs

## Api Security Hash Manager
The Ministry.Web.ApiSecurity library contains a class called **HashManager** which is responsible for doing the hash check work for the attributes described above. You can also use the same class to hash your message body before submitting it if you are doing so through server side code.

Simply create a new instance as follows...

    var hashManager = new HashManager("SHA256");
    
    var hash = hashManager.Hash(valueToHash, secret);

Currently the hash manager will support "SHA256" or "SHA1" hashing. The parameter is case insensitive and can be expanded to add other hashing algorithms.

## The Ministry of Technology Open Source Products
Welcome to The Ministry of Technology open source products. All open source Ministry of Technology products are distributed under the MIT License for maximum re-usability.
Our other open source repositories can be found here...

* [https://github.com/ministryotech](https://github.com/ministryotech)
* [https://github.com/tiefling](https://github.com/tiefling)

### Where can I get it?
You can download the package for this project from any of the following package managers...

- **NUGET** - [https://www.nuget.org/packages/Ministry.Web.ApiSecurity/](https://www.nuget.org/packages/Ministry.Web.ApiSecurity/)

### Contribution guidelines
If you would like to contribute to the project, please contact me.

### Who do I talk to?
* Keith Jackson - temporal-net@live.co.uk
