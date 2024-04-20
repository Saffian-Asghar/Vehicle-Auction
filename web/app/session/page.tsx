import React from 'react'
import { getSession } from '../actions/authActions'

export default async function Session() {
    const session = await getSession();

    return (
        <div>
            <div className='bg-blue-200 border-2 border-blue-500'>
                <h3 className='text-lg'>Session data</h3>
                <pre>{JSON.stringify(session, null, 2)}</pre>
            </div>
        </div>
    )
}