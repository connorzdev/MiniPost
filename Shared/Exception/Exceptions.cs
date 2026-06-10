namespace Shared.Exception;

using System;

public class BadRequestException(string message) : Exception(message);

public class UnAuthorizedException(string message) : Exception(message);

public class ForbiddenException(string message) : Exception(message);

public class NotFoundException(string message) : Exception(message);
