# Apps Service

## Overview
Manages Applications, Subscription Packages, and User Subscriptions.

## Features
- **Subscription Packages**: Defines tiered packages (Weekly, Monthly, Yearly) with features.
- **User Subscriptions**: Tracks active user subscriptions and validity periods.
- **Payment Consumer**: Consumes `PaymentSucceededEvent` to automatically grant/extend subscriptions.
- **Role Management**: Assigns VIP/Premium roles based on active subscriptions.

## Events
- **Consumes**: `PaymentSucceededEvent`
- **Publishes**: `SubscriptionGrantedEvent`
